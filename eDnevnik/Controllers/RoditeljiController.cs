using eDnevnik.Models;
using eDnevnik.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace eDnevnik.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class RoditeljiController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;

        public RoditeljiController(UserManager<Korisnik> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var svi = _userManager.Users.ToList();
            var roditelji = new List<Korisnik>();

            foreach (var korisnik in svi)
            {
                var uloge = await _userManager.GetRolesAsync(korisnik);
                if (uloge.Contains("Roditelj"))
                    roditelji.Add(korisnik);
            }

            return View(roditelji);
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

            var rezultat = await _userManager.CreateAsync(model, "Roditelj123!");

            if (rezultat.Succeeded)
            {
                await _userManager.AddToRoleAsync(model, "Roditelj");
                return RedirectToAction("Index");
            }

            foreach (var error in rezultat.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var roditelj = await _userManager.FindByIdAsync(id);
            if (roditelj == null) return NotFound();

            return View(roditelj);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Korisnik model)
        {
            var roditelj = await _userManager.FindByIdAsync(model.Id);
            if (roditelj == null) return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            roditelj.Ime = model.Ime;
            roditelj.Prezime = model.Prezime;
            roditelj.Email = model.Email;
            roditelj.UserName = model.Email;
            roditelj.Telefon = model.Telefon;
            roditelj.Adresa = model.Adresa;

            await _userManager.UpdateAsync(roditelj);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Obrisi(string id)
        {
            var korisnik = await _userManager.FindByIdAsync(id);
            if (korisnik == null) return NotFound();

            // Provjera da li ima učenika koji ga koriste kao roditelja
            bool imaDjecu = _userManager.Users.Any(u => u.RoditeljId == korisnik.Id);
            if (imaDjecu)
            {
                TempData["Greska"] = "Roditelj ne može biti obrisan jer ima dodijeljene učenike.";
                return RedirectToAction("Index");
            }

            var uloge = await _userManager.GetRolesAsync(korisnik);
            if (!uloge.Contains("Roditelj")) return Forbid();

            await _userManager.DeleteAsync(korisnik);
            return RedirectToAction("Index");
        }
        

    }
}
