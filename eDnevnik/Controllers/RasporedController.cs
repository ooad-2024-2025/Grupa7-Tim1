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
        public async Task<IActionResult> Index(string dan, string nastavnikId, int? razredId)
        {
            var casovi = await _context.Cas
                .Include(c => c.Razred)
                .Include(c => c.Predmet)
                .Include(c => c.Nastavnik)
                .Include(c => c.FixniTermin)
                .Where(c => c.FixniTerminId != null) // Samo novi časovi sa fiksnim terminima
                .ToListAsync();

            var trenutniKorisnik = await _userManager.GetUserAsync(User);
            var uloge = await _userManager.GetRolesAsync(trenutniKorisnik);

            // Filtriraj po ulogama
            if (uloge.Contains("Nastavnik"))
            {
                casovi = casovi.Where(c => c.NastavnikId == trenutniKorisnik.Id).ToList();
                ViewBag.FilterInfo = $"Nastavnik: {trenutniKorisnik.FullName}";
            }
            else if (uloge.Contains("Ucenik"))
            {
                casovi = casovi.Where(c => c.RazredId == trenutniKorisnik.RazredId).ToList();
                ViewBag.FilterInfo = $"Razred: {trenutniKorisnik.Razred?.Naziv}";
            }

            // Dodatni filteri
            if (!string.IsNullOrEmpty(dan))
            {
                var danFilter = Enum.Parse<DayOfWeek>(dan);
                casovi = casovi.Where(c => c.DanUSedmici == danFilter).ToList();
                ViewBag.IzabraniDan = dan;

                var naziviDana = new Dictionary<string, string> {
                    { "Monday", "Ponedjeljak" }, { "Tuesday", "Utorak" }, { "Wednesday", "Srijeda" },
                    { "Thursday", "Četvrtak" }, { "Friday", "Petak" }
                };
                ViewBag.DanNaziv = naziviDana[dan];
            }

            if (!string.IsNullOrEmpty(nastavnikId) && uloge.Contains("Administrator"))
            {
                casovi = casovi.Where(c => c.NastavnikId == nastavnikId).ToList();
                ViewBag.IzabraniNastavnik = nastavnikId;
                var nastavnik = await _userManager.FindByIdAsync(nastavnikId);
                ViewBag.FilterInfo = $"Nastavnik: {nastavnik?.FullName}";
            }

            if (razredId.HasValue && uloge.Contains("Administrator"))
            {
                casovi = casovi.Where(c => c.RazredId == razredId).ToList();
                ViewBag.IzabraniRazred = razredId;
                var razred = await _context.Razred.FindAsync(razredId);
                ViewBag.FilterInfo = $"Razred: {razred?.Naziv}";
            }

            // Pripremi dropdown liste za filtriranje
            await PrepareFilters();

            return View(casovi);
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

        // DODAJ OVE METODE U RasporedController.cs NA KRAJ KLASE (prije zatvorene zagrade)

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

        // HELPER METODA
        private bool CasExists(int id)
        {
            return _context.Cas.Any(e => e.Id == id);
        }

        // HELPER METODE
        private async Task PrepareDropdownLists(Cas cas = null)
        {
            ViewData["RazredId"] = new SelectList(_context.Razred, "Id", "Naziv", cas?.RazredId);
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Naziv", cas?.PredmetId);

            var nastavnici = await _userManager.GetUsersInRoleAsync("Nastavnik");
            ViewData["NastavnikId"] = new SelectList(
                nastavnici.Select(n => new { n.Id, ImePrezime = n.Ime + " " + n.Prezime }),
                "Id", "ImePrezime", cas?.NastavnikId
            );

            var termini = FixniTermin.GetStandardniTermini().Where(t => !t.JeOdmor).ToList();
            ViewData["FixniTerminId"] = new SelectList(termini, "Id", "Naziv", cas?.FixniTerminId);
        }

        private async Task PrepareFilters()
        {
            var nastavnici = await _userManager.GetUsersInRoleAsync("Nastavnik");
            ViewBag.Nastavnici = new SelectList(nastavnici, "Id", "FullName");
            ViewBag.Razredi = new SelectList(_context.Razred, "Id", "Naziv");
        }
    }
}