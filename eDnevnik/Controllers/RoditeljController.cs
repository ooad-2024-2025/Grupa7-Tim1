using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eDnevnik.Models;
using eDnevnik.Data;
using eDnevnik.ViewModels;
using eDnevnik.Data.@enum;

namespace eDnevnik.Controllers
{
    [Authorize(Roles = "Roditelj")]
    public class RoditeljController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ApplicationDbContext _context;

        public RoditeljController(UserManager<Korisnik> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // DASHBOARD - glavna stranica roditelja
        public async Task<IActionResult> Index()
        {
            var roditelj = await _userManager.GetUserAsync(User);
            if (roditelj == null)
                return Unauthorized("Niste prijavljeni.");

            // Dohvati svu djecu ovog roditelja
            var djeca = await _context.Users
                .Include(u => u.Razred)
                .Where(u => u.RoditeljId == roditelj.Id)
                .ToListAsync();

            if (!djeca.Any())
            {
                ViewBag.NemaDjece = true;
                return View(new RoditeljDashboardViewModel { Roditelj = roditelj });
            }

            var djecaIds = djeca.Select(d => d.Id).ToList();

            // Dohvati ocjene za svu djecu
            var ocjene = await _context.Ocjena
                .Include(o => o.Predmet)
                .Include(o => o.Ucenik)
                .Where(o => djecaIds.Contains(o.UcenikId))
                .ToListAsync();

            // Dohvati izostanke za svu djecu
            var izostanci = await _context.Izostanak
                .Include(i => i.Ucenik)
                .Include(i => i.Cas) // DODANO - ovo je nedostajalo
                .Where(i => djecaIds.Contains(i.UcenikId))
                .ToListAsync();

            // Kreiraj DijeteInfo za svako dijete
            var djecaInfo = new List<DijeteInfo>();
            foreach (var dijete in djeca)
            {
                var djeceOcjene = ocjene.Where(o => o.UcenikId == dijete.Id).ToList();
                var djeceIzostanci = izostanci.Where(i => i.UcenikId == dijete.Id).ToList();

                var dijeteInfo = new DijeteInfo
                {
                    Ucenik = dijete,
                    RazredNaziv = dijete.Razred?.Naziv ?? "Nepoznat razred",
                    OpciProsjek = djeceOcjene.Any() ? djeceOcjene.Average(o => o.Vrijednost) : 0,
                    BrojOcjena = djeceOcjene.Count,
                    BrojIzostanaka = djeceIzostanci.Count,
                    VladanjeText = GetVladanjeText(dijete.Vladanje),
                    VladanjeClass = GetVladanjeClass(dijete.Vladanje),
                    NajnovijaOcjena = djeceOcjene.OrderByDescending(o => o.Datum).FirstOrDefault(),
                    PosledneOcjene = djeceOcjene.OrderByDescending(o => o.Datum).Take(3).ToList()
                };

                djecaInfo.Add(dijeteInfo);
            }

            // Kreiraj statistike
            var statistike = new RoditeljStatistike
            {
                UkupnoDjece = djeca.Count,
                ProsjekSveDjece = ocjene.Any() ? ocjene.Average(o => o.Vrijednost) : 0,
                UkupnoOcjena = ocjene.Count,
                UkupnoIzostanaka = izostanci.Count,
                NajboljeDijete = djecaInfo.Where(d => d.OpciProsjek > 0).OrderByDescending(d => d.OpciProsjek).FirstOrDefault(),
                BrojPredmeta = ocjene.Select(o => o.PredmetId).Distinct().Count()
            };

            // Kreiraj recentne aktivnosti
            var recentneAktivnosti = new List<RecentnaAktivnost>();

            // Dodaj recentne ocjene
            foreach (var ocjena in ocjene.OrderByDescending(o => o.Datum).Take(10))
            {
                recentneAktivnosti.Add(new RecentnaAktivnost
                {
                    Tip = "ocjena",
                    Opis = $"Ocjena {ocjena.Vrijednost} iz {ocjena.Predmet?.Naziv}",
                    Datum = ocjena.Datum,
                    DijeteIme = ocjena.Ucenik?.FullName ?? "Nepoznato",
                    IkonaKlasa = "fas fa-star",
                    BojaKlase = ocjena.Vrijednost >= 3 ? "text-success" : "text-danger"
                });
            }

            // Dodaj recentne izostanke - samo ako postoje
            if (izostanci.Any())
            {
                foreach (var izostanak in izostanci.Where(i => i.Cas != null).OrderByDescending(i => i.Cas.Termin).Take(5))
                {
                    recentneAktivnosti.Add(new RecentnaAktivnost
                    {
                        Tip = "izostanak",
                        Opis = $"Izostanak - {GetStatusIzostankaText(izostanak.status)}",
                        Datum = izostanak.Cas.Termin,
                        DijeteIme = izostanak.Ucenik?.FullName ?? "Nepoznato",
                        IkonaKlasa = "fas fa-user-times",
                        BojaKlase = izostanak.status == StatusIzostanka.Opravdan ? "text-warning" : "text-danger"
                    });
                }
            }

            var viewModel = new RoditeljDashboardViewModel
            {
                Roditelj = roditelj,
                Djeca = djecaInfo,
                Statistike = statistike,
                RecentneAktivnosti = recentneAktivnosti.OrderByDescending(a => a.Datum).Take(15).ToList()
            };

            return View(viewModel);
        }

        // OCJENE ODREĐENOG DJETETA
        public async Task<IActionResult> OcjeneDjeteta(string? dijeteId)
        {
            var roditelj = await _userManager.GetUserAsync(User);
            if (roditelj == null)
                return Unauthorized("Niste prijavljeni.");

            Korisnik ucenik;

            if (!string.IsNullOrEmpty(dijeteId))
            {
                // Specifičo dijete
                ucenik = await _context.Users
                    .Include(u => u.Razred)
                    .FirstOrDefaultAsync(u => u.Id == dijeteId && u.RoditeljId == roditelj.Id);
            }
            else
            {
                // Prvo dijete (za backwards compatibility)
                ucenik = await _context.Users
                    .Include(u => u.Razred)
                    .FirstOrDefaultAsync(u => u.RoditeljId == roditelj.Id);
            }

            if (ucenik == null)
                return NotFound("Dijete nije pronađeno.");

            var ocjene = await _context.Ocjena
                .Include(o => o.Predmet)
                .Where(o => o.UcenikId == ucenik.Id)
                .ToListAsync();

            var viewModel = new UcenikOcjeneViewModel
            {
                Ucenik = ucenik,
                OcjenePoPremetima = ocjene
                    .GroupBy(o => o.Predmet)
                    .Select(g => new OcjenaPoPredmetu
                    {
                        Predmet = g.Key,
                        Ocjene = g.ToList(),
                        BrojOcjena = g.Count(),
                        Prosjek = g.Average(x => x.Vrijednost),
                        NajnovijaOcjena = g.OrderByDescending(x => x.Datum).FirstOrDefault()
                    }).ToList(),
                Statistike = new UcenikStatistike
                {
                    OpciProsjek = ocjene.Any() ? ocjene.Average(x => x.Vrijednost) : 0,
                    BrojPredmeta = ocjene.Select(x => x.PredmetId).Distinct().Count(),
                    UkupnoOcjena = ocjene.Count()
                }
            };

            // Dodaj listu sve djece za navigaciju
            ViewBag.SvaDjeca = await _context.Users
                .Where(u => u.RoditeljId == roditelj.Id)
                .Select(u => new { u.Id, u.FullName })
                .ToListAsync();

            ViewBag.TrenutoDijete = ucenik.Id;

            return View(viewModel);
        }

        // IZOSTANCI ODREĐENOG DJETETA
        public async Task<IActionResult> IzostanciDjeteta(string dijeteId)
        {
            var roditelj = await _userManager.GetUserAsync(User);
            if (roditelj == null)
                return Unauthorized("Niste prijavljeni.");

            var ucenik = await _context.Users
                .Include(u => u.Razred)
                .FirstOrDefaultAsync(u => u.Id == dijeteId && u.RoditeljId == roditelj.Id);

            if (ucenik == null)
                return NotFound("Dijete nije pronađeno.");

            var izostanci = await _context.Izostanak
                .Include(i => i.Cas)
                .ThenInclude(c => c.Predmet)
                .Where(i => i.UcenikId == ucenik.Id)
                .OrderByDescending(i => i.Cas.Termin)
                .ToListAsync();

            ViewBag.Ucenik = ucenik;
            return View(izostanci);
        }

        // HELPER METODE
        private string GetVladanjeText(StatusVladanja vladanje)

        {

            return vladanje switch

            {

                StatusVladanja.Primjereno => "Primjerno",

                StatusVladanja.VrloDobro => "Vrlo dobro",

                StatusVladanja.Dobro => "Dobro",

                StatusVladanja.Zadovoljava => "Dovoljno",

                StatusVladanja.Neprimjereno => "Nedovoljno",

                _ => "Neocijenjeno"

            };

        }

        private string GetVladanjeClass(StatusVladanja vladanje)

        {

            return vladanje switch

            {

                StatusVladanja.Primjereno => "text-success",

                StatusVladanja.VrloDobro => "text-success",

                StatusVladanja.Dobro => "text-warning",

                StatusVladanja.Zadovoljava => "text-danger",

                StatusVladanja.Neprimjereno => "text-danger",

                _ => "text-muted"

            };

        }

        private string GetStatusIzostankaText(StatusIzostanka status)
        {
            return status switch
            {
                StatusIzostanka.Opravdan => "Opravdan",
                StatusIzostanka.Neopravdan => "Neopravdan",
                _ => "Nepoznat"
            };
        }
    }
}