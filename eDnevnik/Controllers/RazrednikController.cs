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
    [Authorize(Roles = "Nastavnik")]
    public class RazrednikController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;
        private readonly VladanjeService _vladanjeService;

        public RazrednikController(ApplicationDbContext context, UserManager<Korisnik> userManager, VladanjeService vladanjeService)
        {
            _context = context;
            _userManager = userManager;
            _vladanjeService = vladanjeService;
        }

        // DASHBOARD - Pregled mog razreda
        public async Task<IActionResult> Index()
        {
            var nastavnik = await _userManager.GetUserAsync(User);

            // Pronađi razred gdje je ovaj nastavnik razrednik
            var razred = await _context.Razred
                .FirstOrDefaultAsync(r => r.NastavnikId == nastavnik.Id);

            if (razred == null)
            {
                TempData["Info"] = "Niste dodijeljeni kao razrednik nijednom razredu.";
                return View("NemaRazreda");
            }

            var model = new RazrednikDashboardViewModel
            {
                Razred = razred,
                Nastavnik = nastavnik,
                Ucenici = await GetUceniciRazreda(razred.Id),
                Statistike = await GetStatistikeRazreda(razred.Id),
                VladanjeStatistike = await _vladanjeService.StatistikeVladanjaZaRazred(razred.Id),
                NeodobreniIzostanci = await GetNeodobreneIzostanke(razred.Id)
            };

            return View(model);
        }

        // UČENICI - Lista učenika razreda
        public async Task<IActionResult> Ucenici()
        {
            var nastavnik = await _userManager.GetUserAsync(User);
            var razred = await _context.Razred.FirstOrDefaultAsync(r => r.NastavnikId == nastavnik.Id);

            if (razred == null)
            {
                TempData["Greska"] = "Niste razrednik nijednom razredu.";
                return RedirectToAction("Index");
            }

            var model = new RazrednikUceniciViewModel
            {
                Razred = razred,
                UceniciSaStatistikama = await GetUceniciSaStatistikama(razred.Id)
            };

            return View(model);
        }

        // IZOSTANCI - Upravljanje izostancima
        public async Task<IActionResult> Izostanci(string status = "neodobreni")
        {
            var nastavnik = await _userManager.GetUserAsync(User);
            var razred = await _context.Razred.FirstOrDefaultAsync(r => r.NastavnikId == nastavnik.Id);

            if (razred == null)
            {
                TempData["Greska"] = "Niste razrednik nijednom razredu.";
                return RedirectToAction("Index");
            }

            var model = new RazrednikIzostanciViewModel
            {
                Razred = razred,
                Izostanci = await GetIzostankeRazreda(razred.Id, status),
                SelectedStatus = status
            };

            return View(model);
        }

        // OPRAVDAJ IZOSTANAK
        [HttpPost]
        public async Task<IActionResult> OpravdajIzostanak(int izostanakId)
        {
            var nastavnik = await _userManager.GetUserAsync(User);
            var izostanak = await _context.Izostanak
                .Include(i => i.Ucenik)
                .FirstOrDefaultAsync(i => i.Id == izostanakId);

            if (izostanak == null)
            {
                TempData["Greska"] = "Izostanak nije pronađen.";
                return RedirectToAction("Izostanci");
            }

            // Provjeri da li je nastavnik razrednik ovog učenika
            var razred = await _context.Razred
                .FirstOrDefaultAsync(r => r.NastavnikId == nastavnik.Id && r.Id == izostanak.Ucenik.RazredId);

            if (razred == null)
            {
                TempData["Greska"] = "Nemate dozvolu za opravdavanje ovog izostanka.";
                return RedirectToAction("Izostanci");
            }

            // ISPRAVKA: Opravdaj izostanak i eksplicitno ažuriraj u kontekstu
            izostanak.status = StatusIzostanka.Opravdan;
            _context.Izostanak.Update(izostanak); // DODANO: Eksplicitno Update
            await _context.SaveChangesAsync();

            // Ažuriraj vladanje učenika
            await _vladanjeService.AzurirajVladanjeUcenika(izostanak.UcenikId);

            TempData["Uspjeh"] = $"Izostanak učenika {izostanak.Ucenik.FullName} je opravdan.";
            return RedirectToAction("Izostanci");
        }

        // BULK OPRAVDAVANJE
        [HttpPost]
        public async Task<IActionResult> OpravdajViseIzostanaka(List<int> izostanakIds)
        {
            if (izostanakIds == null || !izostanakIds.Any())
            {
                TempData["Greska"] = "Nije odabran nijedan izostanak.";
                return RedirectToAction("Izostanci");
            }

            var nastavnik = await _userManager.GetUserAsync(User);
            var razred = await _context.Razred.FirstOrDefaultAsync(r => r.NastavnikId == nastavnik.Id);

            if (razred == null)
            {
                TempData["Greska"] = "Niste razrednik nijednom razredu.";
                return RedirectToAction("Izostanci");
            }

            var izostanci = await _context.Izostanak
                .Include(i => i.Ucenik)
                .Where(i => izostanakIds.Contains(i.Id) && i.Ucenik.RazredId == razred.Id)
                .ToListAsync();

            int opravdano = 0;
            var uceniciZaAzuriranje = new HashSet<string>();

            foreach (var izostanak in izostanci)
            {
                if (izostanak.status != StatusIzostanka.Opravdan)
                {
                    izostanak.status = StatusIzostanka.Opravdan;
                    _context.Izostanak.Update(izostanak); // DODANO: Eksplicitno Update
                    uceniciZaAzuriranje.Add(izostanak.UcenikId);
                    opravdano++;
                }
            }

            await _context.SaveChangesAsync();

            // Ažuriraj vladanje svih pogođenih učenika
            foreach (var ucenikId in uceniciZaAzuriranje)
            {
                await _vladanjeService.AzurirajVladanjeUcenika(ucenikId);
            }

            TempData["Uspjeh"] = $"Opravdano je {opravdano} izostanaka.";
            return RedirectToAction("Izostanci");
        }

        // STATISTIKE RAZREDA
        public async Task<IActionResult> Statistike()
        {
            var nastavnik = await _userManager.GetUserAsync(User);
            var razred = await _context.Razred.FirstOrDefaultAsync(r => r.NastavnikId == nastavnik.Id);

            if (razred == null)
            {
                TempData["Greska"] = "Niste razrednik nijednom razredu.";
                return RedirectToAction("Index");
            }

            var model = new RazrednikStatistikeViewModel
            {
                Razred = razred,
                Statistike = await GetDetaljeStatistikeRazreda(razred.Id),
                VladanjeStatistike = await _vladanjeService.StatistikeVladanjaZaRazred(razred.Id),
                OcjenePoPremetima = await GetOcjenePoPremetima(razred.Id)
            };

            return View(model);
        }

        // HELPER METODE
        private async Task<List<UcenikSaStatistikama>> GetUceniciRazreda(int razredId)
        {
            var ucenici = await _userManager.Users
                .Where(u => u.RazredId == razredId)
                .OrderBy(u => u.Prezime).ThenBy(u => u.Ime)
                .ToListAsync();

            var rezultat = new List<UcenikSaStatistikama>();

            foreach (var ucenik in ucenici)
            {
                var roles = await _userManager.GetRolesAsync(ucenik);
                if (roles.Contains("Ucenik"))
                {
                    var statistike = await GetStatistikeUcenika(ucenik.Id);
                    rezultat.Add(new UcenikSaStatistikama
                    {
                        Ucenik = ucenik,
                        OpciProsjek = statistike.OpciProsjek,
                        UkupnoOcjena = statistike.UkupnoOcjena,
                        UkupnoIzostanaka = statistike.UkupnoIzostanaka,
                        NeopravdaniIzostanci = statistike.NeopravdaniIzostanci
                    });
                }
            }

            return rezultat;
        }

        private async Task<List<UcenikSaDetaljnimiStatistikama>> GetUceniciSaStatistikama(int razredId)
        {
            var ucenici = await GetUceniciRazreda(razredId);
            var rezultat = new List<UcenikSaDetaljnimiStatistikama>();

            foreach (var ucenik in ucenici)
            {
                var vladanjeInfo = await _vladanjeService.DetaljiVladanjaUcenika(ucenik.Ucenik.Id);
                rezultat.Add(new UcenikSaDetaljnimiStatistikama
                {
                    Ucenik = ucenik.Ucenik,
                    OpciProsjek = ucenik.OpciProsjek,
                    UkupnoOcjena = ucenik.UkupnoOcjena,
                    UkupnoIzostanaka = ucenik.UkupnoIzostanaka,
                    NeopravdaniIzostanci = ucenik.NeopravdaniIzostanci,
                    VladanjeInfo = vladanjeInfo
                });
            }

            return rezultat.OrderBy(u => u.Ucenik.Prezime).ThenBy(u => u.Ucenik.Ime).ToList();
        }

        private async Task<UcenikStatistike> GetStatistikeUcenika(string ucenikId)
        {
            var ocjene = await _context.Ocjena.Where(o => o.UcenikId == ucenikId).ToListAsync();
            var izostanci = await _context.Izostanak.Where(i => i.UcenikId == ucenikId).ToListAsync();

            return new UcenikStatistike
            {
                OpciProsjek = ocjene.Any() ? ocjene.Average(o => o.Vrijednost) : 0,
                UkupnoOcjena = ocjene.Count,
                UkupnoIzostanaka = izostanci.Count,
                NeopravdaniIzostanci = izostanci.Count(i => i.status == StatusIzostanka.Neopravdan),
                OpravdaniIzostanci = izostanci.Count(i => i.status == StatusIzostanka.Opravdan)
            };
        }

        private async Task<RazredStatistike> GetStatistikeRazreda(int razredId)
        {
            var ucenici = await GetUceniciRazreda(razredId);

            // Filtriranje učenika koji imaju ocjene
            var uceniciSaOcjenama = ucenici.Where(u => u.OpciProsjek > 0).ToList();

            return new RazredStatistike
            {
                BrojUcenika = ucenici.Count,
                ProsjekRazreda = uceniciSaOcjenama.Any() ? uceniciSaOcjenama.Average(u => u.OpciProsjek) : 0,
                UkupnoOcjena = ucenici.Sum(u => u.UkupnoOcjena),
                UkupnoIzostanaka = ucenici.Sum(u => u.UkupnoIzostanaka),
                NeopravdaniIzostanci = ucenici.Sum(u => u.NeopravdaniIzostanci)
            };
        }

        private async Task<RazredDetaljeStatistike> GetDetaljeStatistikeRazreda(int razredId)
        {
            var osnovne = await GetStatistikeRazreda(razredId);
            var ucenici = await GetUceniciRazreda(razredId);

            return new RazredDetaljeStatistike
            {
                Osnovne = osnovne,
                NajboljiUcenik = ucenici.OrderByDescending(u => u.OpciProsjek).FirstOrDefault()?.Ucenik,
                UcenikSaNajviseIzostanaka = ucenici.OrderByDescending(u => u.UkupnoIzostanaka).FirstOrDefault()?.Ucenik,
                BrojUcenikaVrloDobro = ucenici.Count(u => u.OpciProsjek >= 4.5),
                BrojUcenikaDobro = ucenici.Count(u => u.OpciProsjek >= 3.5 && u.OpciProsjek < 4.5),
                BrojUcenikaNedovoljno = ucenici.Count(u => u.OpciProsjek < 2.0 && u.OpciProsjek > 0)
            };
        }

        private async Task<List<IzostanakSaDetaljima>> GetNeodobreneIzostanke(int razredId)
        {
            return await GetIzostankeRazreda(razredId, "neodobreni");
        }

        private async Task<List<IzostanakSaDetaljima>> GetIzostankeRazreda(int razredId, string status)
        {
            var query = _context.Izostanak
                .Include(i => i.Ucenik)
                .Include(i => i.Cas)
                    .ThenInclude(c => c.Predmet)
                .Include(i => i.Cas)
                    .ThenInclude(c => c.FixniTermin)
                .Where(i => i.Ucenik.RazredId == razredId);

            if (status == "neodobreni")
            {
                query = query.Where(i => i.status == StatusIzostanka.Neopravdan);
            }
            else if (status == "odobreni")
            {
                query = query.Where(i => i.status == StatusIzostanka.Opravdan);
            }

            var izostanci = await query.OrderByDescending(i => i.Id).ToListAsync();

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
                    // Fallback - koristi postojeći Termin iz časa ili današnji datum
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

        private async Task<List<OcjenePoPremetima>> GetOcjenePoPremetima(int razredId)
        {
            var predmeti = await _context.PredmetRazred
                .Include(pr => pr.Predmet)
                .Where(pr => pr.RazredId == razredId)
                .Select(pr => pr.Predmet)
                .ToListAsync();

            var rezultat = new List<OcjenePoPremetima>();

            foreach (var predmet in predmeti)
            {
                var ocjene = await _context.Ocjena
                    .Include(o => o.Ucenik)
                    .Where(o => o.PredmetId == predmet.Id && o.Ucenik.RazredId == razredId)
                    .ToListAsync();

                if (ocjene.Any())
                {
                    rezultat.Add(new OcjenePoPremetima
                    {
                        Predmet = predmet,
                        Prosjek = ocjene.Average(o => o.Vrijednost),
                        BrojOcjena = ocjene.Count,
                        Distribucija = ocjene.GroupBy(o => o.Vrijednost)
                                           .ToDictionary(g => g.Key, g => g.Count())
                    });
                }
            }

            return rezultat.OrderBy(o => o.Predmet.Naziv).ToList();
        }
    }
}