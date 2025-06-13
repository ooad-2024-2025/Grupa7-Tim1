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
                .Include(i => i.Cas)
                    .ThenInclude(c => c.Predmet)
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

            // Dodaj recentne izostanke sa pravilnim datumima
            await DodajRecentneIzostanke(recentneAktivnosti, izostanci);

            var viewModel = new RoditeljDashboardViewModel
            {
                Roditelj = roditelj,
                Djeca = djecaInfo,
                Statistike = statistike,
                RecentneAktivnosti = recentneAktivnosti
                    .Where(a => a.Datum > DateTime.MinValue) // Filtriraj aktivnosti sa validnim datumima
                    .OrderByDescending(a => a.Datum)
                    .Take(15)
                    .ToList()
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
        private async Task<List<IzostanakSaDetaljima>> GetIzostanciDjetetaSaTerminima(string dijeteId)
        {
            var izostanci = await _context.Izostanak
                .Include(i => i.Ucenik)
                .Include(i => i.Cas)
                    .ThenInclude(c => c.Predmet)
                .Include(i => i.Cas)
                    .ThenInclude(c => c.FixniTermin)
                .Where(i => i.UcenikId == dijeteId)
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            var rezultat = new List<IzostanakSaDetaljima>();

            foreach (var i in izostanci)
            {
                // Pronađi evidenciju časa da dohvatiš pravi datum
                var evidencija = await _context.EvidencijaCasa
                    .Where(e => e.CasId == i.CasId)
                    .OrderByDescending(e => e.DatumOdrzavanja)
                    .FirstOrDefaultAsync();

                DateTime datumCasa;
                if (evidencija != null)
                {
                    // Ako postoji evidencija, koristi datum iz evidencije + vrijeme iz FixniTermin
                    datumCasa = evidencija.DatumOdrzavanja.Date;
                    if (i.Cas?.FixniTermin != null)
                    {
                        datumCasa = datumCasa.Add(i.Cas.FixniTermin.PocetakVremena);
                    }
                }
                else
                {
                    // Fallback - koristi postojeći Termin iz časa
                    datumCasa = i.Cas?.Termin ?? DateTime.Now;
                }

                rezultat.Add(new IzostanakSaDetaljima
                {
                    Izostanak = i,
                    PredmetNaziv = i.Cas?.Predmet?.Naziv ?? "Nepoznat predmet",
                    TerminInfo = i.Cas?.FixniTermin?.FormatiraniTermin ?? "Nepoznat termin",
                    DatumCasa = datumCasa
                });
            }

            return rezultat;
        }

        public async Task<IActionResult> IzostanciDjeteta(string dijeteId)
        {
            var roditelj = await _userManager.GetUserAsync(User);
            var dijete = await _userManager.FindByIdAsync(dijeteId);

            if (dijete == null || dijete.RoditeljId != roditelj.Id)
            {
                TempData["Greska"] = "Dijete nije pronađeno ili nemate dozvolu.";
                return RedirectToAction("Index");
            }

            // Koristi novu helper metodu
            var izostanci = await GetIzostanciDjetetaSaTerminima(dijete.Id);

            ViewBag.Ucenik = dijete;
            return View(izostanci);
        }

        // HELPER METODA ZA RECENTNE IZOSTANKE
        private async Task DodajRecentneIzostanke(List<RecentnaAktivnost> recentneAktivnosti, List<Izostanak> izostanci)
        {
            // Procesaj maksimalno 15 najnovijih izostanaka
            var izostanciZaProcesiranje = izostanci.OrderByDescending(i => i.Id).Take(15);

            foreach (var izostanak in izostanciZaProcesiranje)
            {
                try
                {
                    DateTime datumIzostanka;

                    // Pronađi evidenciju časa za pravi datum
                    var evidencija = await _context.EvidencijaCasa
                        .Where(e => e.CasId == izostanak.CasId)
                        .OrderByDescending(e => e.DatumOdrzavanja)
                        .FirstOrDefaultAsync();

                    if (evidencija != null)
                    {
                        // Koristi datum iz evidencije
                        datumIzostanka = evidencija.DatumOdrzavanja.Date;

                        // Dodaj vrijeme iz FixniTermin ako postoji
                        var casTermin = await _context.Cas
                            .Include(c => c.FixniTermin)
                            .FirstOrDefaultAsync(c => c.Id == izostanak.CasId);

                        if (casTermin?.FixniTermin != null)
                        {
                            datumIzostanka = datumIzostanka.Add(casTermin.FixniTermin.PocetakVremena);
                        }
                    }
                    else
                    {
                        // Fallback - koristi Cas.Termin ili preskači ako je prazan
                        if (izostanak.Cas?.Termin != null && izostanak.Cas.Termin > DateTime.MinValue)
                        {
                            datumIzostanka = izostanak.Cas.Termin;
                        }
                        else
                        {
                            // Preskači izostanke bez validnog datuma
                            continue;
                        }
                    }

                    // Dodaj samo ako je u zadnjih 30 dana
                    if (datumIzostanka >= DateTime.Now.AddDays(-30))
                    {
                        // Dohvati naziv predmeta
                        string predmetNaziv = "Nepoznat predmet";
                        if (izostanak.Cas?.Predmet != null)
                        {
                            predmetNaziv = izostanak.Cas.Predmet.Naziv;
                        }
                        else if (izostanak.CasId > 0)
                        {
                            var cas = await _context.Cas
                                .Include(c => c.Predmet)
                                .FirstOrDefaultAsync(c => c.Id == izostanak.CasId);
                            predmetNaziv = cas?.Predmet?.Naziv ?? "Nepoznat predmet";
                        }

                        var statusText = GetStatusIzostankaText(izostanak.status);
                        var statusClass = izostanak.status == StatusIzostanka.Opravdan ? "text-warning" : "text-danger";

                        recentneAktivnosti.Add(new RecentnaAktivnost
                        {
                            Tip = "izostanak",
                            Opis = $"Izostanak ({statusText}) - {predmetNaziv}",
                            Datum = datumIzostanka,
                            DijeteIme = izostanak.Ucenik?.FullName ?? "Nepoznato",
                            IkonaKlasa = "fas fa-user-times",
                            BojaKlase = statusClass
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Log greška i nastavi sa sljedećim izostankom
                    Console.WriteLine($"Greška pri procesiranju izostanka {izostanak.Id}: {ex.Message}");
                    continue;
                }
            }
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