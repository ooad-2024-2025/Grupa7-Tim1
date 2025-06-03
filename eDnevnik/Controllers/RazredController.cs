using eDnevnik.Data;
using eDnevnik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace eDnevnik.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class RazrediController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;

        public RazrediController(ApplicationDbContext context, UserManager<Korisnik> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var razredi = _context.Razred.Include(r => r.Nastavnik).ToList();
            return View(razredi);
        }

        [HttpGet]
        public async Task<IActionResult> Dodaj()
        {
            var nastavnici = await _userManager.GetUsersInRoleAsync("Nastavnik");

            ViewBag.Nastavnici = nastavnici.Select(n => new SelectListItem
            {
                Value = n.Id,
                Text = $"{n.Ime} {n.Prezime}"
            }).ToList();

            return View(new Razred());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dodaj(Razred razred)
        {
            if (!ModelState.IsValid)
            {
                var nastavnici = await _userManager.GetUsersInRoleAsync("Nastavnik");
                ViewBag.Nastavnici = nastavnici.Select(n => new SelectListItem
                {
                    Value = n.Id,
                    Text = $"{n.Ime} {n.Prezime}"
                }).ToList();

                return View(razred);
            }

            bool postoji = _context.Razred.Any(r => r.Naziv == razred.Naziv);
            if (postoji)
            {
                ModelState.AddModelError("", "Razred s tim nazivom već postoji.");

                var nastavnici = await _userManager.GetUsersInRoleAsync("Nastavnik");
                ViewBag.Nastavnici = nastavnici.Select(n => new SelectListItem
                {
                    Value = n.Id,
                    Text = $"{n.Ime} {n.Prezime}"
                }).ToList();

                return View(razred);
            }

            _context.Razred.Add(razred);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }




        [HttpGet]
        public async Task<IActionResult> Detalji(int id)
        {
            var razred = await _context.Razred
                .Include(r => r.Nastavnik)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (razred == null)
                return NotFound();

            var ucenici = await _context.Users
                .Where(u => u.RazredId == id)
                .ToListAsync();

            ViewBag.Razred = razred;
            return View(ucenici);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var razred = await _context.Razred.FindAsync(id);
            if (razred == null) return NotFound();

            var nastavnici = await _userManager.GetUsersInRoleAsync("Nastavnik");
            var lista = nastavnici.Select(n => new SelectListItem
            {
                Value = n.Id,
                Text = $"{n.Ime} {n.Prezime}"
            }).ToList();

            ViewBag.Nastavnici = lista;
            return View(razred);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Razred razred)
        {
            if (!ModelState.IsValid)
            {
                var nastavnici = await _userManager.GetUsersInRoleAsync("Nastavnik");
                ViewBag.Nastavnici = nastavnici.Select(n => new SelectListItem
                {
                    Value = n.Id,
                    Text = $"{n.Ime} {n.Prezime}"
                }).ToList();

                return View(razred);
            }

            var postojeci = await _context.Razred.FindAsync(razred.Id);
            if (postojeci == null)
                return NotFound();

            postojeci.Naziv = razred.Naziv;
            postojeci.NastavnikId = razred.NastavnikId;

            _context.Update(postojeci);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var razred = await _context.Razred.FindAsync(id);
            if (razred == null) return NotFound();

            bool imaUcenika = await _context.Users.AnyAsync(u => u.RazredId == id);
            if (imaUcenika)
            {
                TempData["Greska"] = "Nije moguće obrisati razred jer ima učenika.";
                return RedirectToAction(nameof(Index));
            }

            _context.Razred.Remove(razred);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DetaljiPredmeti(int id)
        {
            var razred = await _context.Razred.FindAsync(id);
            if (razred == null) return NotFound();

            var predmeti = await _context.PredmetRazred
                .Include(pr => pr.Predmet)
                    .ThenInclude(p => p.Nastavnik)
                .Where(pr => pr.RazredId == id)
                .ToListAsync();

            ViewBag.RazredNaziv = razred.Naziv;
            ViewBag.RazredId = id;

            var sviPredmeti = await _context.Predmet
                .Include(p => p.Nastavnik)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Naziv + (p.Nastavnik != null ? $" ({p.Nastavnik.Ime} {p.Nastavnik.Prezime})" : "")
                }).ToListAsync();

            ViewBag.SviPredmeti = sviPredmeti;

            return View(predmeti);
        }


        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DodajPredmetURazred(int razredId, int predmetId)
        {
            bool postoji = await _context.PredmetRazred
                .AnyAsync(pr => pr.RazredId == razredId && pr.PredmetId == predmetId);

            if (postoji)
            {
                TempData["Greska"] = "Ovaj predmet je već dodijeljen razredu.";
                return RedirectToAction("DetaljiPredmeti", new { id = razredId });
            }

            if (!postoji)
            {
                var novi = new PredmetRazred
                {
                    RazredId = razredId,
                    PredmetId = predmetId
                };

                _context.PredmetRazred.Add(novi);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("DetaljiPredmeti", new { id = razredId });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ObrisiPredmetIzRazreda(int id, int razredId)
        {
            var entitet = await _context.PredmetRazred.FindAsync(id);
            if (entitet != null)
            {
                _context.PredmetRazred.Remove(entitet);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("DetaljiPredmeti", new { id = razredId });
        }


    }
}
