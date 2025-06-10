using eDnevnik.Data;
using eDnevnik.Models;
using eDnevnik.Services;
using eDnevnik.Data.@enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace eDnevnik.Controllers
{
    [Authorize(Roles = "Nastavnik")]
    public class AktivnostiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;
        private readonly ObavjestenjaService _obavjestenjaService;

        public AktivnostiController(ApplicationDbContext context, UserManager<Korisnik> userManager, ObavjestenjaService obavjestenjaService)
        {
            _context = context;
            _userManager = userManager;
            _obavjestenjaService = obavjestenjaService;
        }

        // PREGLED AKTIVNOSTI
        public async Task<IActionResult> Index(string filter = "nadolazeće")
        {
            var nastavnik = await _userManager.GetUserAsync(User);
            var danas = DateTime.Today;

            var query = _context.Aktivnost
                .Include(a => a.Razred)
                .Include(a => a.Predmet)
                .Where(a => a.NastavnikId == nastavnik.Id);

            query = filter switch
            {
                "prošle" => query.Where(a => a.Datum < danas),
                "danas" => query.Where(a => a.Datum.Date == danas),
                "ovaSedmica" => query.Where(a => a.Datum >= danas && a.Datum <= danas.AddDays(7)),
                "sve" => query,
                _ => query.Where(a => a.Datum >= danas) // nadolazeće (default)
            };

            var aktivnosti = await query
                .OrderBy(a => a.Datum)
                .ToListAsync();

            ViewBag.Filter = filter;
            ViewBag.BrojNadolazećih = await _context.Aktivnost
                .Where(a => a.NastavnikId == nastavnik.Id && a.Datum >= danas)
                .CountAsync();

            return View(aktivnosti);
        }

        // KALENDAR PRIKAZ
        public async Task<IActionResult> Kalendar(int? mjesec = null, int? godina = null)
        {
            var nastavnik = await _userManager.GetUserAsync(User);

            // Ako nisu prosliješeni parametri, koristi trenutni mjesec
            var datumZaKalendar = mjesec.HasValue && godina.HasValue
                ? new DateTime(godina.Value, mjesec.Value, 1)
                : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var pocetakMjeseca = datumZaKalendar;
            var krajMjeseca = pocetakMjeseca.AddMonths(1).AddDays(-1);

            var aktivnosti = await _context.Aktivnost
                .Include(a => a.Razred)
                .Include(a => a.Predmet)
                .Where(a => a.NastavnikId == nastavnik.Id &&
                           a.Datum >= pocetakMjeseca &&
                           a.Datum <= krajMjeseca.AddDays(1)) // Dodaj jedan dan za sigurnost
                .OrderBy(a => a.Datum)
                .ToListAsync();

            // Grupiranje aktivnosti po datumima za kalendar
            var aktivnostiPoDatumima = aktivnosti
                .GroupBy(a => a.Datum.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            ViewBag.AktivnostiPoDatumima = aktivnostiPoDatumima;
            ViewBag.PocetakMjeseca = pocetakMjeseca;
            ViewBag.KrajMjeseca = krajMjeseca;

            return View();
        }

        // DETALJI AKTIVNOSTI
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nastavnik = await _userManager.GetUserAsync(User);
            var aktivnost = await _context.Aktivnost
                .Include(a => a.Razred)
                .Include(a => a.Predmet)
                .Include(a => a.Nastavnik)
                .FirstOrDefaultAsync(a => a.Id == id && a.NastavnikId == nastavnik.Id);

            if (aktivnost == null)
            {
                return NotFound();
            }

            // Dohvati logove obavještenja za ovu aktivnost
            var obavjestenjaLogovi = await _context.ObavjestenjeLog
                .Include(o => o.Korisnik)
                .Where(o => o.AktivnostId == id)
                .OrderByDescending(o => o.VrijemeSlanja)
                .ToListAsync();

            ViewBag.ObavjestenjaLogovi = obavjestenjaLogovi;

            return View(aktivnost);
        }

        // KREIRANJE NOVE AKTIVNOSTI - GET
        public async Task<IActionResult> Create()
        {
            Console.WriteLine("CREATE GET ACTION CALLED");

            var nastavnik = await _userManager.GetUserAsync(User);
            Console.WriteLine($"Nastavnik: {nastavnik?.FullName}");

            // Dohvati razrede
            var razredi = await _context.Razred.OrderBy(r => r.Naziv).ToListAsync();
            ViewBag.Razredi = new SelectList(razredi, "Id", "Naziv");
            Console.WriteLine($"Razredi count: {razredi.Count}");

            // Dohvati predmete koje nastavnik predaje
            var predmeti = await _context.Predmet
                .Where(p => p.NastavnikId == nastavnik.Id)
                .OrderBy(p => p.Naziv)
                .ToListAsync();
            ViewBag.Predmeti = new SelectList(predmeti, "Id", "Naziv");
            Console.WriteLine($"Predmeti count: {predmeti.Count}");

            // Enum vrijednosti
            ViewBag.TipAktivnosti = new SelectList(
                Enum.GetValues<TipAktivnosti>().Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = x switch
                    {
                        TipAktivnosti.Test => "Test",
                        TipAktivnosti.Zadaca => "Zadaća",
                        TipAktivnosti.Takmicenje => "Takmičenje",
                        TipAktivnosti.SkolarskiDogadjaj => "Školski događaj",
                        TipAktivnosti.Prezentacija => "Prezentacija",
                        TipAktivnosti.Ekskurzija => "Ekskurzija",
                        _ => x.ToString()
                    }
                }),
                "Value", "Text"
            );

            ViewBag.PrioritetAktivnosti = new SelectList(
                Enum.GetValues<PrioritetAktivnosti>().Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = x switch
                    {
                        PrioritetAktivnosti.Visok => "Visok (odmah obavještenje)",
                        PrioritetAktivnosti.Srednji => "Srednji (3 dana prije)",
                        PrioritetAktivnosti.Nizak => "Nizak (1 dan prije)",
                        _ => x.ToString()
                    }
                }),
                "Value", "Text"
            );

            Console.WriteLine("ViewBag data prepared successfully");
            return View();
        }

        // KREIRANJE NOVE AKTIVNOSTI - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Naziv,Opis,Datum,Tip,Prioritet,RazredId,PredmetId")] Aktivnost aktivnost)
        {
            var nastavnik = await _userManager.GetUserAsync(User);
            aktivnost.NastavnikId = nastavnik.Id;
            aktivnost.DatumKreiranja = DateTime.Now;
            aktivnost.Aktivna = true;

            // Validacija datuma
            if (aktivnost.Datum <= DateTime.Now)
            {
                ModelState.AddModelError("Datum", "Datum aktivnosti mora biti u budućnosti.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(aktivnost);
                await _context.SaveChangesAsync();

                // Ako je prioritet visok, pošalji obavještenje odmah
                if (aktivnost.Prioritet == PrioritetAktivnosti.Visok)
                {
                    try
                    {
                        var brojPoslanih = await _obavjestenjaService.PošaljiObavještenjeZaAktivnost(aktivnost);
                        TempData["Uspjeh"] = $"Aktivnost je kreirana i poslano je {brojPoslanih} obavještenja.";
                    }
                    catch (Exception ex)
                    {
                        TempData["Greška"] = "Aktivnost je kreirana, ali došlo je do greške prilikom slanja obavještenja.";
                    }
                }
                else
                {
                    TempData["Uspjeh"] = "Aktivnost je uspješno kreirana. Obavještenja će biti poslana automatski prema prioritetu.";
                }

                return RedirectToAction(nameof(Index));
            }

            // Ako ModelState nije valjan, ponovo učitaj podatke za dropdown-ove
            await LoadDropdownData(nastavnik);
            return View(aktivnost);
        }

        // UREĐIVANJE AKTIVNOSTI - GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nastavnik = await _userManager.GetUserAsync(User);
            var aktivnost = await _context.Aktivnost
                .FirstOrDefaultAsync(a => a.Id == id && a.NastavnikId == nastavnik.Id);

            if (aktivnost == null)
            {
                return NotFound();
            }

            await LoadDropdownData(nastavnik);
            return View(aktivnost);
        }

        // UREĐIVANJE AKTIVNOSTI - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Naziv,Opis,Datum,Tip,Prioritet,RazredId,PredmetId,Aktivna")] Aktivnost aktivnost)
        {
            if (id != aktivnost.Id)
            {
                return NotFound();
            }

            var nastavnik = await _userManager.GetUserAsync(User);
            var postojecaAktivnost = await _context.Aktivnost
                .FirstOrDefaultAsync(a => a.Id == id && a.NastavnikId == nastavnik.Id);

            if (postojecaAktivnost == null)
            {
                return NotFound();
            }

            // Validacija datuma
            if (aktivnost.Datum <= DateTime.Now && aktivnost.Aktivna)
            {
                ModelState.AddModelError("Datum", "Datum aktivne aktivnosti mora biti u budućnosti.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Ažuriraj samo dopuštena polja
                    postojecaAktivnost.Naziv = aktivnost.Naziv;
                    postojecaAktivnost.Opis = aktivnost.Opis;
                    postojecaAktivnost.Datum = aktivnost.Datum;
                    postojecaAktivnost.Tip = aktivnost.Tip;
                    postojecaAktivnost.Prioritet = aktivnost.Prioritet;
                    postojecaAktivnost.RazredId = aktivnost.RazredId;
                    postojecaAktivnost.PredmetId = aktivnost.PredmetId;
                    postojecaAktivnost.Aktivna = aktivnost.Aktivna;

                    _context.Update(postojecaAktivnost);
                    await _context.SaveChangesAsync();

                    TempData["Uspjeh"] = "Aktivnost je uspješno ažurirana.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AktivnostExists(aktivnost.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownData(nastavnik);
            return View(aktivnost);
        }

        // BRISANJE AKTIVNOSTI - GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nastavnik = await _userManager.GetUserAsync(User);
            var aktivnost = await _context.Aktivnost
                .Include(a => a.Razred)
                .Include(a => a.Predmet)
                .FirstOrDefaultAsync(a => a.Id == id && a.NastavnikId == nastavnik.Id);

            if (aktivnost == null)
            {
                return NotFound();
            }

            return View(aktivnost);
        }

        // BRISANJE AKTIVNOSTI - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nastavnik = await _userManager.GetUserAsync(User);
            var aktivnost = await _context.Aktivnost
                .FirstOrDefaultAsync(a => a.Id == id && a.NastavnikId == nastavnik.Id);

            if (aktivnost != null)
            {
                // Obriši povezane logove obavještenja
                var logovi = await _context.ObavjestenjeLog
                    .Where(o => o.AktivnostId == id)
                    .ToListAsync();

                _context.ObavjestenjeLog.RemoveRange(logovi);
                _context.Aktivnost.Remove(aktivnost);
                await _context.SaveChangesAsync();

                TempData["Uspjeh"] = "Aktivnost je uspješno obrisana.";
            }

            return RedirectToAction(nameof(Index));
        }

        // SLANJE OBAVJEŠTENJA ODMAH
        [HttpPost]
        public async Task<IActionResult> PošaljiObavještenjeSada(int id)
        {
            var nastavnik = await _userManager.GetUserAsync(User);
            var aktivnost = await _context.Aktivnost
                .Include(a => a.Razred)
                .Include(a => a.Predmet)
                .FirstOrDefaultAsync(a => a.Id == id && a.NastavnikId == nastavnik.Id);

            if (aktivnost == null)
            {
                TempData["Greška"] = "Aktivnost nije pronađena.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var brojPoslanih = await _obavjestenjaService.PošaljiObavještenjeZaAktivnost(aktivnost);
                TempData["Uspjeh"] = $"Poslano je {brojPoslanih} obavještenja.";
            }
            catch (Exception ex)
            {
                TempData["Greška"] = "Došlo je do greške prilikom slanja obavještenja.";
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // SLANJE OBAVJEŠTENJA RUČNO
        [HttpPost]
        public async Task<IActionResult> PošaljiObavještenje(int id)
        {
            var nastavnik = await _userManager.GetUserAsync(User);
            var aktivnost = await _context.Aktivnost
                .Include(a => a.Razred)
                .Include(a => a.Predmet)
                .FirstOrDefaultAsync(a => a.Id == id && a.NastavnikId == nastavnik.Id);

            if (aktivnost == null)
            {
                return Json(new { success = false, message = "Aktivnost nije pronađena." });
            }

            try
            {
                var brojPoslanih = await _obavjestenjaService.PošaljiObavještenjeZaAktivnost(aktivnost);
                return Json(new { success = true, message = $"Poslano je {brojPoslanih} obavještenja." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Došlo je do greške prilikom slanja obavještenja." });
            }
        }

        // DEAKTIVIRANJE/AKTIVIRANJE AKTIVNOSTI
        [HttpPost]
        public async Task<IActionResult> ToggleAktivnost(int id)
        {
            var nastavnik = await _userManager.GetUserAsync(User);
            var aktivnost = await _context.Aktivnost
                .FirstOrDefaultAsync(a => a.Id == id && a.NastavnikId == nastavnik.Id);

            if (aktivnost == null)
            {
                return Json(new { success = false, message = "Aktivnost nije pronađena." });
            }

            aktivnost.Aktivna = !aktivnost.Aktivna;
            await _context.SaveChangesAsync();

            var status = aktivnost.Aktivna ? "aktivirana" : "deaktivirana";
            return Json(new { success = true, message = $"Aktivnost je {status}.", aktivna = aktivnost.Aktivna });
        }

        // STATISTIKE OBAVJEŠTENJA
        public async Task<IActionResult> StatistikeObavještenja()
        {
            var nastavnik = await _userManager.GetUserAsync(User);

            var statistike = await _context.ObavjestenjeLog
                .Include(o => o.Aktivnost)
                .Where(o => o.Aktivnost.NastavnikId == nastavnik.Id)
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Broj = g.Count()
                })
                .ToListAsync();

            var ukupnoObavještenja = await _context.ObavjestenjeLog
                .Include(o => o.Aktivnost)
                .Where(o => o.Aktivnost.NastavnikId == nastavnik.Id)
                .CountAsync();

            var neuspješnaObavještenja = await _context.ObavjestenjeLog
                .Include(o => o.Aktivnost)
                .Include(o => o.Korisnik)
                .Where(o => o.Aktivnost.NastavnikId == nastavnik.Id && o.Status == StatusObavjestenja.Greška)
                .OrderByDescending(o => o.VrijemeSlanja)
                .ToListAsync();

            ViewBag.Statistike = statistike;
            ViewBag.UkupnoObavještenja = ukupnoObavještenja;
            ViewBag.NeuspješnaObavještenja = neuspješnaObavještenja;

            return View();
        }

        // PONOVI NEUSPJEŠNA OBAVJEŠTENJA
        [HttpPost]
        public async Task<IActionResult> PonociObavještenja()
        {
            try
            {
                var ponovljeno = await _obavjestenjaService.PonociNeuspješnaObavještenja();
                return Json(new { success = true, message = $"Ponovljeno je {ponovljeno} obavještenja." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Došlo je do greške prilikom ponovnog slanja." });
            }
        }

        // HELPER METODE
        private async Task LoadDropdownData(Korisnik nastavnik)
        {
            ViewBag.Razredi = new SelectList(
                await _context.Razred.OrderBy(r => r.Naziv).ToListAsync(),
                "Id", "Naziv"
            );

            ViewBag.Predmeti = new SelectList(
                await _context.Predmet
                    .Where(p => p.NastavnikId == nastavnik.Id)
                    .OrderBy(p => p.Naziv)
                    .ToListAsync(),
                "Id", "Naziv"
            );

            ViewBag.TipAktivnosti = new SelectList(
                Enum.GetValues<TipAktivnosti>().Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = x switch
                    {
                        TipAktivnosti.Test => "Test",
                        TipAktivnosti.Zadaca => "Zadaća",
                        TipAktivnosti.Takmicenje => "Takmičenje",
                        TipAktivnosti.SkolarskiDogadjaj => "Školski događaj",
                        TipAktivnosti.Prezentacija => "Prezentacija",
                        TipAktivnosti.Ekskurzija => "Ekskurzija",
                        _ => x.ToString()
                    }
                }),
                "Value", "Text"
            );

            ViewBag.PrioritetAktivnosti = new SelectList(
                Enum.GetValues<PrioritetAktivnosti>().Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = x switch
                    {
                        PrioritetAktivnosti.Visok => "Visok (odmah obavještenje)",
                        PrioritetAktivnosti.Srednji => "Srednji (3 dana prije)",
                        PrioritetAktivnosti.Nizak => "Nizak (1 dan prije)",
                        _ => x.ToString()
                    }
                }),
                "Value", "Text"
            );
        }

        private bool AktivnostExists(int id)
        {
            return _context.Aktivnost.Any(e => e.Id == id);
        }

        // API ENDPOINTS ZA AJAX POZIVE
        [HttpGet]
        public async Task<IActionResult> GetAktivnostiZaKalendar(DateTime pocetakMjeseca, DateTime krajMjeseca)
        {
            var nastavnik = await _userManager.GetUserAsync(User);

            var aktivnosti = await _context.Aktivnost
                .Include(a => a.Razred)
                .Include(a => a.Predmet)
                .Where(a => a.NastavnikId == nastavnik.Id &&
                           a.Datum >= pocetakMjeseca &&
                           a.Datum <= krajMjeseca)
                .Select(a => new
                {
                    id = a.Id,
                    title = a.Naziv,
                    start = a.Datum.ToString("yyyy-MM-dd HH:mm:ss"),
                    className = a.TipClass,
                    tip = a.TipText,
                    opis = a.Opis,
                    prioritet = a.PrioritetText,
                    razred = a.Razred != null ? a.Razred.Naziv : "Svi razredi",
                    predmet = a.Predmet != null ? a.Predmet.Naziv : ""
                })
                .ToListAsync();

            return Json(aktivnosti);
        }
    }
}