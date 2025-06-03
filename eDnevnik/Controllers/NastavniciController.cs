using eDnevnik.Data;
using eDnevnik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eDnevnik.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class NastavniciController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ApplicationDbContext _context;

        public NastavniciController(UserManager<Korisnik> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var sviKorisnici = _userManager.Users.ToList();
            var nastavnici = new List<Korisnik>();

            foreach (var korisnik in sviKorisnici)
            {
                var role = await _userManager.GetRolesAsync(korisnik);
                if (role.Contains("Nastavnik"))
                    nastavnici.Add(korisnik);
            }

            return View(nastavnici);
        }

        [HttpGet]
        public IActionResult Dodaj()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Dodaj(Korisnik model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.UserName = model.Email;
            model.EmailConfirmed = true;

            var rezultat = await _userManager.CreateAsync(model, "Nastavnik123!");

            if (rezultat.Succeeded)
            {
                await _userManager.AddToRoleAsync(model, "Nastavnik");
                return RedirectToAction("Index");
            }

            foreach (var error in rezultat.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var nastavnik = await _userManager.FindByIdAsync(id);
            if (nastavnik == null) return NotFound();

            return View(nastavnik);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Korisnik model)
        {
            var nastavnik = await _userManager.FindByIdAsync(model.Id);
            if (nastavnik == null) return NotFound();

            if (!ModelState.IsValid) return View(model);

            nastavnik.Ime = model.Ime;
            nastavnik.Prezime = model.Prezime;
            nastavnik.Email = model.Email;
            nastavnik.UserName = model.Email;
            nastavnik.Telefon = model.Telefon;
            nastavnik.Adresa = model.Adresa;

            await _userManager.UpdateAsync(nastavnik);
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Obrisi(string id)
        {
            var korisnik = await _userManager.FindByIdAsync(id);
            if (korisnik == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(korisnik);
            if (!roles.Contains("Nastavnik"))
                return Forbid();

            // Provjera: da li je razrednik
            bool jeRazrednik = _context.Razred.Any(r => r.NastavnikId == korisnik.Id);

            if (jeRazrednik)
            {
                TempData["Greska"] = "Nije moguće obrisati nastavnika jer je razrednik u jednom ili više razreda.";
                return RedirectToAction("Index");
            }

            await _userManager.DeleteAsync(korisnik);
            return RedirectToAction("Index");

        }

    }
}
