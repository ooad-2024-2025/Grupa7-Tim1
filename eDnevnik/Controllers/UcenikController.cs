using eDnevnik.Data;
using eDnevnik.Models;
using eDnevnik.ViewModels;
using eDnevnik.Services;
using eDnevnik.Data.@enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eDnevnik.Controllers
{
    [Authorize(Roles = "Ucenik")]
    public class UcenikController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;
        private readonly VladanjeService _vladanjeService;

        public UcenikController(ApplicationDbContext context, UserManager<Korisnik> userManager, VladanjeService vladanjeService)
        {
            _context = context;
            _userManager = userManager;
            _vladanjeService = vladanjeService;
        }

        // DASHBOARD - Glavni pregled
        public async Task<IActionResult> Index()
        {
            var ucenik = await _userManager.GetUserAsync(User);
            if (ucenik?.RazredId == null)
            {
                TempData["Greska"] = "Niste dodijeljeni nijednom razredu.";
                return View("Error");
            }

            // Ažuriraj vladanje
            await _vladanjeService.AzurirajVladanjeUcenika(ucenik.Id);

            var model = new UcenikDashboardViewModel
            {
                Ucenik = ucenik,
                OcjenePoPremetima = await GetOcjenePoPremetima(ucenik.Id),
                RecentniIzostanci = await GetRecentneIzostanke(ucenik.Id, 5),
                VladanjeInfo = await _vladanjeService.DetaljiVladanjaUcenika(ucenik.Id),
                Statistike = await GetUcenikStatistike(ucenik.Id),
                RecentneAktivnosti = await GetRecentneAktivnosti(ucenik.Id, 10),
                DanasnjiCasovi = await GetDanasnjiCasovi(ucenik.RazredId.Value)
            };

            return View(model);
        }

        // OCJENE - Detaljni pregled ocjena
        public async Task<IActionResult> Ocjene(string predmet = "svi", DateTime? odDatuma = null, DateTime? doDatuma = null)
        {
            var ucenik = await _userManager.GetUserAsync(User);
            if (ucenik?.RazredId == null) return RedirectToAction("Index");

            var model = new UcenikOcjeneViewModel
            {
                Ucenik = ucenik,
                OcjenePoPremetima = await GetOcjenePoPremetima(ucenik.Id, predmet, odDatuma, doDatuma),
                Statistike = await GetUcenikStatistike(ucenik.Id),
                SelectedPredmet = predmet,
                OdDatuma = odDatuma,
                DoDatuma = doDatuma
            };

            // Dropdown lista predmeta
            var predmeti = await _context.PredmetRazred
                .Include(pr => pr.Predmet)
                .Where(pr => pr.RazredId == ucenik.RazredId)
                .Select(pr => pr.Predmet)
                .Distinct()
                .ToListAsync();

            ViewBag.Predmeti = predmeti;

            return View(model);
        }

        // IZOSTANCI - Detaljni pregled izostanaka
        public async Task<IActionResult> Izostanci(string status = "svi", DateTime? odDatuma = null, DateTime? doDatuma = null)
        {
            var ucenik = await _userManager.GetUserAsync(User);
            if (ucenik?.RazredId == null) return RedirectToAction("Index");

            // Ažuriraj vladanje
            await _vladanjeService.AzurirajVladanjeUcenika(ucenik.Id);

            var model = new UcenikIzostanciViewModel
            {
                Ucenik = ucenik,
                Izostanci = await GetIzostanciSaDetaljima(ucenik.Id, status, odDatuma, doDatuma),
                VladanjeInfo = await _vladanjeService.DetaljiVladanjaUcenika(ucenik.Id),
                IzostanciPoMjesecima = await GetIzostanciPoMjesecima(ucenik.Id),
                SelectedStatus = status,
                OdDatuma = odDatuma,
                DoDatuma = doDatuma
            };

            return View(model);
        }

        // RASPORED - Pregled rasporeda za učenika
        public async Task<IActionResult> Raspored()
        {
            var ucenik = await _userManager.GetUserAsync(User);
            if (ucenik?.RazredId == null) return RedirectToAction("Index");

            return RedirectToAction("Index", "Raspored");
        }

        // PRIVATE HELPER METODE
        private async Task<List<OcjenaPoPredmetu>> GetOcjenePoPremetima(string ucenikId, string filterPredmet = "svi", DateTime? odDatuma = null, DateTime? doDatuma = null)
        {
            var ocjene = await _context.Ocjena
                .Include(o => o.Predmet)
                .Where(o => o.UcenikId == ucenikId)
                .ToListAsync();

            // Filtriraj po datumu
            if (odDatuma.HasValue)
                ocjene = ocjene.Where(o => o.Datum.Date >= odDatuma.Value.Date).ToList();
            if (doDatuma.HasValue)
                ocjene = ocjene.Where(o => o.Datum.Date <= doDatuma.Value.Date).ToList();

            var poPremetima = ocjene
                .GroupBy(o => o.Predmet)
                .Select(g => new OcjenaPoPredmetu
                {
                    Predmet = g.Key,
                    Ocjene = g.OrderByDescending(o => o.Datum).ToList(),
                    Prosjek = g.Average(o => o.Vrijednost),
                    BrojOcjena = g.Count(),
                    NajnovijaOcjena = g.OrderByDescending(o => o.Datum).FirstOrDefault()
                })
                .OrderBy(p => p.Predmet?.Naziv)
                .ToList();

            // Filtriraj po predmetu
            if (filterPredmet != "svi" && int.TryParse(filterPredmet, out int predmetId))
            {
                poPremetima = poPremetima.Where(p => p.Predmet?.Id == predmetId).ToList();
            }

            return poPremetima;
        }

        private async Task<List<Izostanak>> GetRecentneIzostanke(string ucenikId, int broj)
        {
            return await _context.Izostanak
                .Include(i => i.Cas)
                    .ThenInclude(c => c.Predmet)
                .Include(i => i.Cas)
                    .ThenInclude(c => c.FixniTermin)
                .Where(i => i.UcenikId == ucenikId)
                .OrderByDescending(i => i.Id)
                .Take(broj)
                .ToListAsync();
        }

        private async Task<UcenikStatistike> GetUcenikStatistike(string ucenikId)
        {
            var ocjene = await _context.Ocjena
                .Where(o => o.UcenikId == ucenikId)
                .ToListAsync();

            var izostanci = await _context.Izostanak
                .Where(i => i.UcenikId == ucenikId)
                .ToListAsync();

            var ucenik = await _userManager.FindByIdAsync(ucenikId);
            var razredId = ucenik?.RazredId;

            var predmeti = await _context.PredmetRazred
                .Include(pr => pr.Predmet)
                .Where(pr => pr.RazredId == razredId.Value)
                .Select(pr => pr.Predmet)
                .Distinct()
                .CountAsync();

            var distribucija = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                distribucija[i] = ocjene.Count(o => o.Vrijednost == i);
            }

            return new UcenikStatistike
            {
                OpciProsjek = ocjene.Any() ? ocjene.Average(o => o.Vrijednost) : 0,
                UkupnoOcjena = ocjene.Count,
                UkupnoIzostanaka = izostanci.Count,
                NeopravdaniIzostanci = izostanci.Count(i => i.status == StatusIzostanka.Neopravdan),
                OpravdaniIzostanci = izostanci.Count(i => i.status == StatusIzostanka.Opravdan),
                BrojPredmeta = predmeti,
                OcjeneDistribucija = distribucija
            };
        }

        private async Task<List<ActivityItem>> GetRecentneAktivnosti(string ucenikId, int broj)
        {
            var aktivnosti = new List<ActivityItem>();

            // Dodaj nedavne ocjene
            var ocjene = await _context.Ocjena
                .Include(o => o.Predmet)
                .Where(o => o.UcenikId == ucenikId)
                .OrderByDescending(o => o.Datum)
                .Take(broj / 2)
                .ToListAsync();

            foreach (var ocjena in ocjene)
            {
                aktivnosti.Add(new ActivityItem
                {
                    Tip = "Ocjena",
                    Opis = $"Ocjena {ocjena.Vrijednost} iz predmeta {ocjena.Predmet?.Naziv}",
                    Datum = ocjena.Datum,
                    Ikona = "fas fa-star",
                    CssClass = ocjena.Vrijednost >= 3 ? "text-success" : "text-danger"
                });
            }

            // Dodaj nedavne izostanke
            var izostanci = await _context.Izostanak
                .Include(i => i.Cas)
                    .ThenInclude(c => c.Predmet)
                .Include(i => i.Cas)
                    .ThenInclude(c => c.FixniTermin)
                .Where(i => i.UcenikId == ucenikId)
                .OrderByDescending(i => i.Id)
                .Take(broj / 2)
                .ToListAsync();

            foreach (var izostanak in izostanci)
            {
                aktivnosti.Add(new ActivityItem
                {
                    Tip = "Izostanak",
                    Opis = $"Izostanak sa časa {izostanak.Cas?.Predmet?.Naziv} ({izostanak.status})",
                    Datum = DateTime.Now, // Nema datum u modelu, koristimo trenutni
                    Ikona = "fas fa-user-times",
                    CssClass = izostanak.status == StatusIzostanka.Opravdan ? "text-warning" : "text-danger"
                });
            }

            return aktivnosti.OrderByDescending(a => a.Datum).Take(broj).ToList();
        }

        private async Task<List<Cas>> GetDanasnjiCasovi(int razredId)
        {
            var danas = DateTime.Today.DayOfWeek;

            return await _context.Cas
                .Include(c => c.Predmet)
                .Include(c => c.FixniTermin)
                .Include(c => c.Nastavnik)
                .Where(c => c.RazredId == razredId &&
                           c.DanUSedmici == danas &&
                           c.FixniTerminId != null)
                .OrderBy(c => c.FixniTermin.Redoslijed)
                .ToListAsync();
        }

        private async Task<List<IzostanakSaDetaljima>> GetIzostanciSaDetaljima(string ucenikId, string statusFilter, DateTime? odDatuma, DateTime? doDatuma)
        {
            var izostanci = await _context.Izostanak
                .Include(i => i.Cas)
                    .ThenInclude(c => c.Predmet)
                .Include(i => i.Cas)
                    .ThenInclude(c => c.FixniTermin)
                .Where(i => i.UcenikId == ucenikId)
                .ToListAsync();

            // Filtriraj po statusu
            if (statusFilter != "svi")
            {
                if (Enum.TryParse<StatusIzostanka>(statusFilter, out var status))
                {
                    izostanci = izostanci.Where(i => i.status == status).ToList();
                }
            }

            var rezultat = izostanci.Select(i => new IzostanakSaDetaljima
            {
                Izostanak = i,
                PredmetNaziv = i.Cas?.Predmet?.Naziv ?? "Nepoznat predmet",
                TerminInfo = i.Cas?.FixniTermin?.FormatiraniTermin ?? "Nepoznat termin",
                // ISPRAVKA: Kombinuj dan u sedmici sa vremenom fiksnog termina
                DatumCasa = IzracunajDatumCasa(i.Cas)
            }).ToList();

            // Filtriraj po datumu
            if (odDatuma.HasValue)
                rezultat = rezultat.Where(r => r.DatumCasa.Date >= odDatuma.Value.Date).ToList();
            if (doDatuma.HasValue)
                rezultat = rezultat.Where(r => r.DatumCasa.Date <= doDatuma.Value.Date).ToList();

            return rezultat.OrderByDescending(r => r.DatumCasa).ToList();
        }

        // Helper metoda za izračunavanje datuma časa
        private DateTime IzracunajDatumCasa(Cas? cas)
        {
            if (cas?.DanUSedmici == null || cas.FixniTermin == null)
                return DateTime.Now;

            // Pronađi poslednji odgovarajući dan u sedmici
            var danas = DateTime.Today;
            var danasnjiDan = danas.DayOfWeek;
            var ciljaniDan = cas.DanUSedmici.Value;

            // Izračunaj razliku u danima
            int razlikaDana = (int)ciljaniDan - (int)danasnjiDan;

            // Ako je dan već prošao u ovoj sedmici, uzmi iz prošle sedmice
            if (razlikaDana > 0)
                razlikaDana -= 7;

            var datumCasa = danas.AddDays(razlikaDana);

            // Dodaj vrijeme iz fiksnog termina
            var datumSaVremenom = datumCasa.Add(cas.FixniTermin.PocetakVremena);

            return datumSaVremenom;
        }

        private async Task<Dictionary<string, int>> GetIzostanciPoMjesecima(string ucenikId)
        {
            var izostanci = await _context.Izostanak
                .Include(i => i.Cas)
                    .ThenInclude(c => c.FixniTermin)
                .Where(i => i.UcenikId == ucenikId)
                .ToListAsync();

            var mjeseci = new Dictionary<string, int>();
            string[] naziviMjeseci = { "Januar", "Februar", "Mart", "April", "Maj", "Jun",
                             "Juli", "Avgust", "Septembar", "Oktobar", "Novembar", "Decembar" };

            // Inicijaliziraj sve mjesece na 0
            for (int i = 0; i < 12; i++)
            {
                mjeseci[naziviMjeseci[i]] = 0;
            }

            // Grupiraj izostanke po mjesecima na osnovu izračunatog datuma časa
            foreach (var izostanak in izostanci)
            {
                var datumCasa = IzracunajDatumCasa(izostanak.Cas);
                var mjesec = datumCasa.Month - 1; // -1 jer array počinje od 0

                if (mjesec >= 0 && mjesec < 12)
                {
                    mjeseci[naziviMjeseci[mjesec]]++;
                }
            }

            return mjeseci;
        }
    }
}