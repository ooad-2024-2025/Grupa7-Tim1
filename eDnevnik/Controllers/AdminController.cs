using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eDnevnik.Models;
using eDnevnik.ViewModels;
using eDnevnik.Data;

namespace eDnevnik.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<Korisnik> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var korisnici = _userManager.Users.ToList();
            var korisniciView = new List<KorisnikViewModel>();

            foreach (var korisnik in korisnici)
            {
                var uloge = await _userManager.GetRolesAsync(korisnik);
                korisniciView.Add(new KorisnikViewModel
                {
                    Id = korisnik.Id,
                    Ime = korisnik.Ime,
                    Prezime = korisnik.Prezime,
                    Email = korisnik.Email,
                    Uloge = string.Join(", ", uloge)
                });
            }

            var brojUcenika = await _userManager.GetUsersInRoleAsync("Ucenik");
            var brojNastavnika = await _userManager.GetUsersInRoleAsync("Nastavnik");
            var brojRoditelja = await _userManager.GetUsersInRoleAsync("Roditelj");

            var brojRazreda = await _context.Razred.CountAsync();
            var brojPredmeta = await _context.Predmet.CountAsync();

            var dashboard = new AdminDashboardViewModel
            {
                BrojUcenika = brojUcenika.Count,
                BrojNastavnika = brojNastavnika.Count,
                BrojRoditelja = brojRoditelja.Count,
                BrojRazreda = brojRazreda,
                BrojPredmeta = brojPredmeta,
                Korisnici = korisniciView
            };

            return View(dashboard);
        }
    }
}
