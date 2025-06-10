using eDnevnik.Data;
using eDnevnik.Models;
using eDnevnik.Data.@enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eDnevnik.Controllers
{
    [Authorize(Roles = "Nastavnik")]
    public class EvidencijaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;

        public EvidencijaController(ApplicationDbContext context, UserManager<Korisnik> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // PREGLED ČASOVA ZA EVIDENTIRANJE
        public async Task<IActionResult> Index()
        {
            var nastavnik = await _userManager.GetUserAsync(User);

            // Dohvati časove nastavnika za današnji dan i buduće
            var danas = DateTime.Today;
            var casovi = await _context.Cas
                .Include(c => c.Razred)
                .Include(c => c.Predmet)
                .Include(c => c.FixniTermin)
                .Where(c => c.NastavnikId == nastavnik.Id &&
                           c.FixniTerminId != null) // Samo novi časovi sa fiksnim terminima
                .ToListAsync();

            // Filtriraj časove za današnji dan (na osnovu DanUSedmici)
            var danasnjiDan = danas.DayOfWeek;
            var danasnjiCasovi = casovi.Where(c => c.DanUSedmici == danasnjiDan).ToList();

            // Dohvati već evidentirane časove za danas
            var evidencije = await _context.EvidencijaCasa
                .Where(e => e.NastavnikId == nastavnik.Id &&
                           e.DatumOdrzavanja.Date == danas)
                .Select(e => e.CasId)
                .ToListAsync();

            // Označi koji časovi su već evidentirani
            foreach (var cas in danasnjiCasovi)
            {
                ViewData[$"Evidentiran_{cas.Id}"] = evidencije.Contains(cas.Id);
            }

            ViewBag.Danas = danas.ToString("dddd, dd.MM.yyyy");
            return View(danasnjiCasovi);
        }

        // EVIDENTIRANJE ČASA - GLAVNI INTERFEJS
        [HttpGet]
        public async Task<IActionResult> Evidentiranje(int casId)
        {
            var nastavnik = await _userManager.GetUserAsync(User);

            var cas = await _context.Cas
                .Include(c => c.Razred)
                .Include(c => c.Predmet)
                .Include(c => c.FixniTermin)
                .FirstOrDefaultAsync(c => c.Id == casId && c.NastavnikId == nastavnik.Id);

            if (cas == null)
            {
                TempData["Greska"] = "Čas nije pronađen ili nemate dozvolu.";
                return RedirectToAction("Index");
            }

            // Provjeri da li je čas već evidentiran danas
            var danas = DateTime.Today;
            var postojecaEvidencija = await _context.EvidencijaCasa
                .FirstOrDefaultAsync(e => e.CasId == casId &&
                                         e.DatumOdrzavanja.Date == danas);

            if (postojecaEvidencija != null)
            {
                TempData["Greska"] = "Čas je već evidentiran za današnji dan.";
                return RedirectToAction("Index");
            }

            // Dohvati učenike razreda
            var ucenici = await _userManager.Users
                .Where(u => u.RazredId == cas.RazredId)
                .OrderBy(u => u.Prezime).ThenBy(u => u.Ime)
                .ToListAsync();

            // Provjeri da li su učenici
            var uceniciFiltered = new List<Korisnik>();
            foreach (var korisnik in ucenici)
            {
                var roles = await _userManager.GetRolesAsync(korisnik);
                if (roles.Contains("Ucenik"))
                {
                    uceniciFiltered.Add(korisnik);
                }
            }

            ViewBag.Cas = cas;
            ViewBag.Ucenici = uceniciFiltered;

            // Kreiraj prazan model za evidenciju
            var model = new EvidencijaCasa
            {
                CasId = casId,
                NastavnikId = nastavnik.Id,
                DatumOdrzavanja = DateTime.Now
            };

            return View(model);
        }

        // SNIMANJE EVIDENCIJE ČASA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Evidentiranje(EvidencijaCasa evidencija,
            List<string> prisutniUcenici,
            List<string> ucenikIdZaOcjenu,
            List<int> ocjene,
            List<string> komentariOcjena,
            List<string> komentariIzostanci)
        {
            var nastavnik = await _userManager.GetUserAsync(User);

            var cas = await _context.Cas
                .Include(c => c.Razred)
                .Include(c => c.Predmet)
                .FirstOrDefaultAsync(c => c.Id == evidencija.CasId && c.NastavnikId == nastavnik.Id);

            if (cas == null)
            {
                TempData["Greska"] = "Čas nije pronađen.";
                return RedirectToAction("Index");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. SNIMAJ EVIDENCIJU ČASA
                evidencija.NastavnikId = nastavnik.Id;
                evidencija.VrijemeEvidentiranja = DateTime.Now;
                _context.EvidencijaCasa.Add(evidencija);
                await _context.SaveChangesAsync();

                // 2. STVORI IZOSTANKE ZA NEPRISUTNE UČENIKE
                var sviUcenici = await _userManager.Users
                    .Where(u => u.RazredId == cas.RazredId)
                    .ToListAsync();

                var uceniciIds = new List<string>();
                foreach (var korisnik in sviUcenici)
                {
                    var roles = await _userManager.GetRolesAsync(korisnik);
                    if (roles.Contains("Ucenik"))
                    {
                        uceniciIds.Add(korisnik.Id);
                    }
                }

                if (prisutniUcenici == null) prisutniUcenici = new List<string>();

                var neprisutniUcenici = uceniciIds.Except(prisutniUcenici).ToList();

                for (int i = 0; i < neprisutniUcenici.Count; i++)
                {
                    var izostanak = new Izostanak
                    {
                        UcenikId = neprisutniUcenici[i],
                        CasId = cas.Id,
                        status = StatusIzostanka.Neopravdan,
                        Komentar = i < komentariIzostanci?.Count ? komentariIzostanci[i] : null
                    };
                    _context.Izostanak.Add(izostanak);
                }

                // 3. UNESI OCJENE
                if (ucenikIdZaOcjenu != null && ocjene != null)
                {
                    for (int i = 0; i < ucenikIdZaOcjenu.Count && i < ocjene.Count; i++)
                    {
                        if (ocjene[i] >= 1 && ocjene[i] <= 5) // Validiraj ocjenu
                        {
                            var ocjena = new Ocjena
                            {
                                UcenikId = ucenikIdZaOcjenu[i],
                                PredmetId = cas.PredmetId,
                                Vrijednost = ocjene[i],
                                Komentar = i < komentariOcjena?.Count ? komentariOcjena[i] : null,
                                Datum = DateTime.Now
                            };
                            _context.Ocjena.Add(ocjena);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Uspjeh"] = $"Čas {cas.Predmet?.Naziv} za razred {cas.Razred?.Naziv} je uspješno evidentiran!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Greska"] = "Greška prilikom snimanja evidencije: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // PREGLED EVIDENCIJA
        public async Task<IActionResult> Pregled(DateTime? datum)
        {
            var nastavnik = await _userManager.GetUserAsync(User);
            var targetDatum = datum ?? DateTime.Today;

            var evidencije = await _context.EvidencijaCasa
                .Include(e => e.Cas)
                    .ThenInclude(c => c.Razred)
                .Include(e => e.Cas)
                    .ThenInclude(c => c.Predmet)
                .Include(e => e.Cas)
                    .ThenInclude(c => c.FixniTermin)
                .Where(e => e.NastavnikId == nastavnik.Id &&
                           e.DatumOdrzavanja.Date == targetDatum.Date)
                .OrderBy(e => e.Cas.FixniTermin.Redoslijed)
                .ToListAsync();

            ViewBag.TargetDatum = targetDatum;
            return View(evidencije);
        }

        // DETALJI EVIDENCIJE
        public async Task<IActionResult> Detalji(int id)
        {
            var nastavnik = await _userManager.GetUserAsync(User);

            var evidencija = await _context.EvidencijaCasa
                .Include(e => e.Cas)
                    .ThenInclude(c => c.Razred)
                .Include(e => e.Cas)
                    .ThenInclude(c => c.Predmet)
                .Include(e => e.Cas)
                    .ThenInclude(c => c.FixniTermin)
                .FirstOrDefaultAsync(e => e.Id == id && e.NastavnikId == nastavnik.Id);

            if (evidencija == null)
            {
                return NotFound();
            }

            // Dohvati izostanke za ovaj čas na ovaj datum
            var izostanci = await _context.Izostanak
                .Include(i => i.Ucenik)
                .Where(i => i.CasId == evidencija.CasId)
                .ToListAsync();

            // Dohvati ocjene date na ovaj datum za ovaj predmet
            var ocjene = await _context.Ocjena
                .Include(o => o.Ucenik)
                .Where(o => o.PredmetId == evidencija.Cas.PredmetId &&
                           o.Datum.Date == evidencija.DatumOdrzavanja.Date)
                .ToListAsync();

            ViewBag.Izostanci = izostanci;
            ViewBag.Ocjene = ocjene;

            return View(evidencija);
        }
    }
}