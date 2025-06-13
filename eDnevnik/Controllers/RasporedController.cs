using eDnevnik.Data;
using eDnevnik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace eDnevnik.Controllers
{
    public class RasporedController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;

        public RasporedController(ApplicationDbContext context, UserManager<Korisnik> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // PREGLED RASPOREDA - glavni sedmični prikaz
        [Authorize]
        public async Task<IActionResult> Index(string? dan, int? razredId)
        {
            var casovi = new List<Cas>(); // Počni sa praznom listom

            // Samo za Administrator prikaži selektor razreda
            if (User.IsInRole("Administrator"))
            {
                var razredi = await _context.Razred
                    .OrderBy(r => r.Naziv)
                    .Select(r => new SelectListItem
                    {
                        Value = r.Id.ToString(),
                        Text = r.Naziv,
                        Selected = r.Id == razredId
                    }).ToListAsync();

                // Dodaj "Odaberi razred" opciju na vrh
                razredi.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Odaberi razred --",
                    Selected = !razredId.HasValue
                });

                ViewBag.Razredi = razredi;

                // Učitaj časove SAMO ako je razred odabran
                if (razredId.HasValue)
                {
                    casovi = await _context.Cas
                        .Include(c => c.Predmet)
                        .Include(c => c.Razred)
                        .Include(c => c.Nastavnik)
                        .Include(c => c.FixniTermin)
                        .Where(c => c.RazredId == razredId.Value && c.FixniTerminId != null)
                        .ToListAsync();

                    var odabraniRazred = await _context.Razred.FindAsync(razredId.Value);
                    ViewBag.FilterInfo = $"Raspored za razred: {odabraniRazred?.Naziv}";
                }
                else
                {
                    ViewBag.FilterInfo = "Odaberite razred za prikaz rasporeda";
                }
            }
            else if (User.IsInRole("Nastavnik"))
            {
                var nastavnikId = _userManager.GetUserId(User);
                casovi = await _context.Cas
                    .Include(c => c.Predmet)
                    .Include(c => c.Razred)
                    .Include(c => c.Nastavnik)
                    .Include(c => c.FixniTermin)
                    .Where(c => c.NastavnikId == nastavnikId && c.FixniTerminId != null)
                    .ToListAsync();

                ViewBag.FilterInfo = "Vaš raspored časova";
            }
            else if (User.IsInRole("Ucenik"))
            {
                var ucenik = await _userManager.GetUserAsync(User);
                if (ucenik?.RazredId != null)
                {
                    casovi = await _context.Cas
                        .Include(c => c.Predmet)
                        .Include(c => c.Razred)
                        .Include(c => c.Nastavnik)
                        .Include(c => c.FixniTermin)
                        .Where(c => c.RazredId == ucenik.RazredId && c.FixniTerminId != null)
                        .ToListAsync();

                    ViewBag.FilterInfo = $"Raspored za razred: {ucenik.Razred?.Naziv}";
                }
            }

            // Filtriraj po danu ako je odabran
            if (!string.IsNullOrEmpty(dan) && Enum.TryParse<DayOfWeek>(dan, out var dayOfWeek))
            {
                casovi = casovi.Where(c => c.DanUSedmici == dayOfWeek).ToList();
                ViewBag.IzabraniDan = dan;
                ViewBag.DanNaziv = GetDanNaziv(dayOfWeek);

                if (!string.IsNullOrEmpty(ViewBag.FilterInfo as string))
                {
                    ViewBag.FilterInfo += $" - {GetDanNaziv(dayOfWeek)}";
                }
            }

            return View(casovi);
        }

        // AJAX METODA - Dohvati predmete za odabrani razred
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetPredmetiZaRazred(int razredId)
        {
            var predmeti = await _context.PredmetRazred
                .Include(pr => pr.Predmet)
                .Where(pr => pr.RazredId == razredId)
                .Select(pr => new {
                    id = pr.Predmet.Id,
                    naziv = pr.Predmet.Naziv
                })
                .OrderBy(p => p.naziv)
                .ToListAsync();

            return Json(predmeti);
        }

        // ADMIN FUNKCIJE - upravljanje rasporedom
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Upravljanje()
        {
            var casovi = await _context.Cas
                .Include(c => c.Razred)
                .Include(c => c.Predmet)
                .Include(c => c.Nastavnik)
                .Include(c => c.FixniTermin)
                .Where(c => c.FixniTerminId != null)
                .OrderBy(c => c.DanUSedmici)
                .ThenBy(c => c.FixniTermin.Redoslijed)
                .ToListAsync();

            return View(casovi);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create()
        {
            await PrepareDropdownLists();
            return View(new Cas());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(Cas cas)
        {
            // Provjeri da li je predmet dodijeljen odabranom razredu
            var predmetJeDodijeljen = await _context.PredmetRazred
                .AnyAsync(pr => pr.RazredId == cas.RazredId && pr.PredmetId == cas.PredmetId);

            if (!predmetJeDodijeljen)
            {
                ModelState.AddModelError("PredmetId", "Odabrani predmet nije dodijeljen ovom razredu.");
            }

            // Provjeri da li već postoji čas u istom terminu za taj razred
            var postojeciCas = await _context.Cas
                .AnyAsync(c => c.RazredId == cas.RazredId &&
                          c.DanUSedmici == cas.DanUSedmici &&
                          c.FixniTerminId == cas.FixniTerminId);

            if (postojeciCas)
            {
                ModelState.AddModelError("FixniTerminId", "Razred već ima čas u tom terminu.");
            }

            // Provjeri da li nastavnik već drži čas u istom terminu
            var nastavnikZauzet = await _context.Cas
                .AnyAsync(c => c.NastavnikId == cas.NastavnikId &&
                          c.DanUSedmici == cas.DanUSedmici &&
                          c.FixniTerminId == cas.FixniTerminId);

            if (nastavnikZauzet)
            {
                ModelState.AddModelError("FixniTerminId", "Nastavnik već drži čas u tom terminu.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(cas);
                await _context.SaveChangesAsync();
                TempData["Uspjeh"] = "Čas je uspješno dodan.";
                return RedirectToAction(nameof(Upravljanje));
            }

            await PrepareDropdownLists(cas);
            return View(cas);
        }

        // EDIT ČASA
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cas = await _context.Cas
                .Include(c => c.Razred)
                .Include(c => c.Predmet)
                .Include(c => c.Nastavnik)
                .Include(c => c.FixniTermin)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cas == null)
            {
                return NotFound();
            }

            await PrepareDropdownLists(cas);
            return View(cas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, Cas cas)
        {
            if (id != cas.Id)
            {
                return NotFound();
            }

            // Provjeri da li je predmet dodijeljen odabranom razredu
            var predmetJeDodijeljen = await _context.PredmetRazred
                .AnyAsync(pr => pr.RazredId == cas.RazredId && pr.PredmetId == cas.PredmetId);

            if (!predmetJeDodijeljen)
            {
                ModelState.AddModelError("PredmetId", "Odabrani predmet nije dodijeljen ovom razredu.");
            }

            // Provjeri da li već postoji čas u istom terminu za taj razred (osim trenutnog)
            var postojeciCas = await _context.Cas
                .AnyAsync(c => c.Id != cas.Id &&
                          c.RazredId == cas.RazredId &&
                          c.DanUSedmici == cas.DanUSedmici &&
                          c.FixniTerminId == cas.FixniTerminId);

            if (postojeciCas)
            {
                ModelState.AddModelError("FixniTerminId", "Razred već ima čas u tom terminu.");
            }

            // Provjeri da li nastavnik već drži čas u istom terminu (osim trenutnog)
            var nastavnikZauzet = await _context.Cas
                .AnyAsync(c => c.Id != cas.Id &&
                          c.NastavnikId == cas.NastavnikId &&
                          c.DanUSedmici == cas.DanUSedmici &&
                          c.FixniTerminId == cas.FixniTerminId);

            if (nastavnikZauzet)
            {
                ModelState.AddModelError("FixniTerminId", "Nastavnik već drži čas u tom terminu.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cas);
                    await _context.SaveChangesAsync();
                    TempData["Uspjeh"] = "Čas je uspješno ažuriran.";
                    return RedirectToAction(nameof(Upravljanje));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CasExists(cas.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            await PrepareDropdownLists(cas);
            return View(cas);
        }

        // DELETE ČASA
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cas = await _context.Cas
                .Include(c => c.Razred)
                .Include(c => c.Predmet)
                .Include(c => c.Nastavnik)
                .Include(c => c.FixniTermin)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cas == null)
            {
                return NotFound();
            }

            return View(cas);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cas = await _context.Cas.FindAsync(id);
            if (cas != null)
            {
                _context.Cas.Remove(cas);
                await _context.SaveChangesAsync();
                TempData["Uspjeh"] = "Čas je uspješno obrisan.";
            }

            return RedirectToAction(nameof(Upravljanje));
        }

        // HELPER METODE
        private bool CasExists(int id)
        {
            return _context.Cas.Any(e => e.Id == id);
        }

        private async Task PrepareDropdownLists(Cas cas = null)
        {
            ViewData["RazredId"] = new SelectList(_context.Razred, "Id", "Naziv", cas?.RazredId);

            // Za Edit akciju - učitaj predmete samo za odabrani razred
            if (cas?.RazredId != null)
            {
                var predmetiZaRazred = await _context.PredmetRazred
                    .Include(pr => pr.Predmet)
                    .Where(pr => pr.RazredId == cas.RazredId)
                    .Select(pr => pr.Predmet)
                    .ToListAsync();

                ViewData["PredmetId"] = new SelectList(predmetiZaRazred, "Id", "Naziv", cas?.PredmetId);
            }
            else
            {
                // Za Create akciju - prazan dropdown (popunit će se JavaScript-om)
                ViewData["PredmetId"] = new SelectList(new List<Predmet>(), "Id", "Naziv");
            }

            var nastavnici = await _userManager.GetUsersInRoleAsync("Nastavnik");
            ViewData["NastavnikId"] = new SelectList(
                nastavnici.Select(n => new { n.Id, ImePrezime = n.Ime + " " + n.Prezime }),
                "Id", "ImePrezime", cas?.NastavnikId
            );

            var termini = FixniTermin.GetStandardniTermini().Where(t => !t.JeOdmor).ToList();
            ViewData["FixniTerminId"] = new SelectList(termini, "Id", "Naziv", cas?.FixniTerminId);
        }

        private string GetDanNaziv(DayOfWeek dan)
        {
            return dan switch
            {
                DayOfWeek.Monday => "Ponedjeljak",
                DayOfWeek.Tuesday => "Utorak",
                DayOfWeek.Wednesday => "Srijeda",
                DayOfWeek.Thursday => "Četvrtak",
                DayOfWeek.Friday => "Petak",
                _ => "Nepoznat dan"
            };
        }
    }
}