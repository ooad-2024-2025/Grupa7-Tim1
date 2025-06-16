using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using eDnevnik.Models;

namespace eDnevnik.Controllers
{
    [Authorize] // Ovo će zahtijevati da korisnik bude prijavljen za sve akcije
    public class HomeController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;

        public HomeController(UserManager<Korisnik> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Sada ne treba provjera User.Identity.IsAuthenticated jer je [Authorize] već to osigurao
            var user = await _userManager.GetUserAsync(User);

            // Dodaj null check za slučaj da user nije pronađen
            if (user == null)
            {
                // Koristi Challenge() umjesto RedirectToAction jer nemaš Account controller
                return Challenge();
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Preusmjeri korisnike prema njihovoj ulozi
            if (roles.Contains("Administrator"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (roles.Contains("Nastavnik"))
            {
                return RedirectToAction("Index", "Evidencija"); // ili neki nastavnički dashboard
            }
            else if (roles.Contains("Roditelj"))
            {
                return RedirectToAction("Index", "Roditelj");
            }
            else if (roles.Contains("Ucenik"))
            {
                return RedirectToAction("Index", "Ucenik");
            }

            // Ako korisnik nema definisanu ulogu, pokaži default home page
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous] // Error stranica može biti dostupna svima
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}