using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using eDnevnik.Models;
using eDnevnik.Data;

namespace eDnevnik.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UceniciController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ApplicationDbContext _context;

        public UceniciController(UserManager<Korisnik> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sviKorisnici = await _userManager.Users
                .Include(x => x.Razred)
                .Include(x => x.Roditelj)
                .ToListAsync();

            var ucenici = new List<Korisnik>();
            foreach (var korisnik in sviKorisnici)
            {
                var role = await _userManager.GetRolesAsync(korisnik);
                if (role.Contains("Ucenik"))
                    ucenici.Add(korisnik);
            }

            return View(ucenici);
        }

        [HttpGet]
        public async Task<IActionResult> Dodaj()
        {
            ViewBag.Razredi = new SelectList(_context.Razred.ToList(), "Id", "Naziv");

            var sviKorisnici = await _userManager.Users.ToListAsync();
            var roditelji = new List<Korisnik>();
            foreach (var korisnik in sviKorisnici)
            {
                var role = await _userManager.GetRolesAsync(korisnik);
                if (role.Contains("Roditelj"))
                    roditelji.Add(korisnik);
            }

            ViewBag.Roditelji = new SelectList(roditelji, "Id", "Email");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Dodaj(Korisnik model)
        {
            if (model.RazredId == null)
                ModelState.AddModelError("RazredId", "Obavezno je odabrati razred.");

            if (string.IsNullOrEmpty(model.RoditeljId))
                ModelState.AddModelError("RoditeljId", "Obavezno je odabrati roditelja.");

            var postojeci = await _userManager.FindByEmailAsync(model.Email);
            if (postojeci != null)
                ModelState.AddModelError("Email", "Korisnik sa datim emailom već postoji.");

            if (!ModelState.IsValid)
            {
                ViewBag.Razredi = new SelectList(_context.Razred.ToList(), "Id", "Naziv");

                var sviKorisnici = await _userManager.Users.ToListAsync();
                var roditelji = new List<Korisnik>();
                foreach (var korisnik in sviKorisnici)
                {
                    var role = await _userManager.GetRolesAsync(korisnik);
                    if (role.Contains("Roditelj"))
                        roditelji.Add(korisnik);
                }
                ViewBag.Roditelji = new SelectList(roditelji, "Id", "Email");

                return View(model);
            }

            model.UserName = model.Email;
            model.EmailConfirmed = true;

            var rezultat = await _userManager.CreateAsync(model, "Ucenik123!");

            if (rezultat.Succeeded)
            {
                await _userManager.AddToRoleAsync(model, "Ucenik");
                return RedirectToAction("Index");
            }

            foreach (var error in rezultat.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            ViewBag.Razredi = new SelectList(_context.Razred.ToList(), "Id", "Naziv");

            var sviRoditelji = await _userManager.Users.ToListAsync();
            var roditeljiPonovo = new List<Korisnik>();
            foreach (var korisnik in sviRoditelji)
            {
                var role = await _userManager.GetRolesAsync(korisnik);
                if (role.Contains("Roditelj"))
                    roditeljiPonovo.Add(korisnik);
            }
            ViewBag.Roditelji = new SelectList(roditeljiPonovo, "Id", "Email");

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var ucenik = await _userManager.Users
                .Include(u => u.Razred)
                .Include(u => u.Roditelj)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (ucenik == null)
                return NotFound();

            ViewBag.Razredi = new SelectList(_context.Razred.ToList(), "Id", "Naziv", ucenik.RazredId);

            var sviKorisnici = await _userManager.Users.ToListAsync();
            var roditelji = new List<Korisnik>();
            foreach (var korisnik in sviKorisnici)
            {
                var role = await _userManager.GetRolesAsync(korisnik);
                if (role.Contains("Roditelj"))
                    roditelji.Add(korisnik);
            }
            ViewBag.Roditelji = new SelectList(roditelji, "Id", "Email", ucenik.RoditeljId);

            return View(ucenik);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Korisnik model)
        {
            var ucenik = await _userManager.FindByIdAsync(model.Id);
            if (ucenik == null)
                return NotFound();

            if (model.RazredId == null)
                ModelState.AddModelError("RazredId", "Obavezno je odabrati razred.");

            if (string.IsNullOrEmpty(model.RoditeljId))
                ModelState.AddModelError("RoditeljId", "Obavezno je odabrati roditelja.");

            if (!ModelState.IsValid)
            {
                ViewBag.Razredi = new SelectList(_context.Razred.ToList(), "Id", "Naziv", model.RazredId);

                var sviKorisnici = await _userManager.Users.ToListAsync();
                var roditelji = new List<Korisnik>();
                foreach (var korisnik in sviKorisnici)
                {
                    var role = await _userManager.GetRolesAsync(korisnik);
                    if (role.Contains("Roditelj"))
                        roditelji.Add(korisnik);
                }
                ViewBag.Roditelji = new SelectList(roditelji, "Id", "Email", model.RoditeljId);

                return View(model);
            }

            ucenik.Ime = model.Ime;
            ucenik.Prezime = model.Prezime;
            ucenik.Email = model.Email;
            ucenik.UserName = model.Email;
            ucenik.Telefon = model.Telefon;
            ucenik.Adresa = model.Adresa;
            ucenik.RazredId = model.RazredId;
            ucenik.RoditeljId = model.RoditeljId;

            await _userManager.UpdateAsync(ucenik);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Obrisi(string id)
        {
            var korisnik = await _userManager.FindByIdAsync(id);
            if (korisnik == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(korisnik);
            if (!roles.Contains("Ucenik"))
                return Forbid();

            await _userManager.DeleteAsync(korisnik);
            return RedirectToAction("Index");
        }
    }
}
