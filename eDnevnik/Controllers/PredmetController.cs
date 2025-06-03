using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eDnevnik.Data;
using eDnevnik.Models;

namespace eDnevnik.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class PredmetiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;

        public PredmetiController(ApplicationDbContext context, UserManager<Korisnik> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var predmeti = _context.Predmet
                .Include(p => p.Nastavnik)
                .ToList();

            return View(predmeti);
        }

        [HttpGet]
        public async Task<IActionResult> Dodaj()
        {
            var nastavnici = await _userManager.GetUsersInRoleAsync("Nastavnik");
            ViewBag.Nastavnici = nastavnici;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Dodaj(Predmet predmet)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Nastavnici = await _userManager.GetUsersInRoleAsync("Nastavnik");
                return View(predmet);
            }

            _context.Predmet.Add(predmet);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var predmet = await _context.Predmet.FindAsync(id);
            if (predmet == null)
                return NotFound();

            ViewBag.Nastavnici = await _userManager.GetUsersInRoleAsync("Nastavnik");
            return View(predmet);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Predmet predmet)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Nastavnici = await _userManager.GetUsersInRoleAsync("Nastavnik");
                return View(predmet);
            }

            _context.Predmet.Update(predmet);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var predmet = await _context.Predmet.FindAsync(id);
            if (predmet == null)
                return NotFound();

            _context.Predmet.Remove(predmet);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
