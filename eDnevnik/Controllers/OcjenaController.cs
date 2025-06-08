using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using eDnevnik.Data;
using eDnevnik.Models;

namespace eDnevnik.Controllers
{
    [Authorize(Roles = "Nastavnik")]
    public class OcjenaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;

        public OcjenaController(ApplicationDbContext context, UserManager<Korisnik> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Razredi()
        {
            var user = await _userManager.GetUserAsync(User);
            var razredi = await _context.PredmetRazred
                .Include(pr => pr.Razred)
                .Include(pr => pr.Predmet)
                .Where(pr => pr.Predmet.NastavnikId == user.Id)
                .Select(pr => pr.Razred)
                .Distinct()
                .ToListAsync();

            return View(razredi);
        }

        [HttpGet]
        public async Task<IActionResult> Predmeti(int razredId)
        {
            var user = await _userManager.GetUserAsync(User);
            var predmeti = await _context.PredmetRazred
                .Include(pr => pr.Predmet)
                .Where(pr => pr.RazredId == razredId && pr.Predmet.NastavnikId == user.Id)
                .Select(pr => pr.Predmet)
                .ToListAsync();

            ViewBag.RazredId = razredId;
            return View(predmeti);
        }

        [HttpGet]
        public async Task<IActionResult> Ucenici(int razredId, int predmetId)
        {
            var ucenici = await _userManager.Users
                .Where(u => u.RazredId == razredId)
                .ToListAsync();

            ViewBag.PredmetId = predmetId;
            ViewBag.RazredId = razredId;
            return View(ucenici);
        }

        [HttpGet]
        public async Task<IActionResult> Dodaj(string ucenikId, int predmetId)
        {
            var ucenik = await _userManager.FindByIdAsync(ucenikId);
            var predmet = await _context.Predmet.FindAsync(predmetId);

            if (ucenik == null || predmet == null)
                return NotFound();

            ViewBag.Ucenik = ucenik;
            ViewBag.Predmet = predmet;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Dodaj(string UcenikId, int PredmetId, int Vrijednost, string? Komentar)
        {
            if (Vrijednost < 1 || Vrijednost > 5)
            {
                ModelState.AddModelError("Vrijednost", "Ocjena mora biti između 1 i 5.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Ucenik = await _userManager.FindByIdAsync(UcenikId);
                ViewBag.Predmet = await _context.Predmet.FindAsync(PredmetId);
                return View();
            }

            var ocjena = new Ocjena
            {
                UcenikId = UcenikId,
                PredmetId = PredmetId,
                Vrijednost = Vrijednost,
                Komentar = Komentar,
                Datum = DateTime.Now
            };

            _context.Ocjena.Add(ocjena);
            await _context.SaveChangesAsync();

            return RedirectToAction("Razredi");
        }

        [HttpGet]
        public async Task<IActionResult> Unos()
        {
            var nastavnik = await _userManager.GetUserAsync(User);

            var predmetiIds = await _context.Predmet
                .Where(p => p.NastavnikId == nastavnik.Id)
                .Select(p => p.Id)
                .ToListAsync();

            var dodjele = await _context.PredmetRazred
                .Include(pr => pr.Razred)
                .Include(pr => pr.Predmet)
                .Where(pr => predmetiIds.Contains(pr.PredmetId))
                .ToListAsync();

            ViewBag.Dodjele = dodjele
                .Select(d => new SelectListItem
                {
                    Value = $"{d.RazredId}_{d.PredmetId}",
                    Text = $"{d.Razred.Naziv} - {d.Predmet.Naziv}"
                })
                .ToList();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> UnesiZaRazredPredmet(string dodjela)
        {
            if (string.IsNullOrEmpty(dodjela) || !dodjela.Contains('_'))
                return RedirectToAction("Unos");

            var parts = dodjela.Split('_');
            int razredId = int.Parse(parts[0]);
            int predmetId = int.Parse(parts[1]);

            var predmet = await _context.Predmet.FindAsync(predmetId);
            var razred = await _context.Razred.FindAsync(razredId);

            var ucenici = await _userManager.Users
                .Where(u => u.RazredId == razredId)
                .ToListAsync();

            ViewBag.Predmet = predmet;
            ViewBag.Razred = razred;
            ViewBag.PredmetId = predmetId;
            ViewBag.RazredId = razredId;

            return View(ucenici);
        }

        [HttpPost]
        public async Task<IActionResult> SpasiOcjene(int PredmetId, List<string> UcenikIdList, List<int> Vrijednosti, List<string?> Komentari)
        {
            if (UcenikIdList.Count != Vrijednosti.Count)
                return BadRequest("Neusklađeni podaci.");

            for (int i = 0; i < UcenikIdList.Count; i++)
            {
                var ocjena = new Ocjena
                {
                    UcenikId = UcenikIdList[i],
                    PredmetId = PredmetId,
                    Vrijednost = Vrijednosti[i],
                    Komentar = Komentari[i],
                    Datum = DateTime.Now
                };

                _context.Ocjena.Add(ocjena);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Unos");
        }

        [HttpGet]
        public async Task<IActionResult> Rezime(int razredId, int predmetId)
        {
            var nastavnik = await _userManager.GetUserAsync(User);

            var ocjene = await _context.Ocjena
                .Include(o => o.Ucenik)
                .Include(o => o.Predmet)
                .Where(o => o.PredmetId == predmetId && o.Ucenik.RazredId == razredId && o.Predmet.NastavnikId == nastavnik.Id)
                .ToListAsync();

            ViewBag.Razred = await _context.Razred.FindAsync(razredId);
            ViewBag.Predmet = await _context.Predmet.FindAsync(predmetId);
            ViewBag.RazredId = razredId;
            ViewBag.PredmetId = predmetId;

            return View(ocjene);
        }

        [HttpGet]
        public async Task<IActionResult> Uredi(int id)
        {
            var ocjena = await _context.Ocjena.Include(o => o.Ucenik).Include(o => o.Predmet).FirstOrDefaultAsync(o => o.Id == id);
            if (ocjena == null) return NotFound();

            ViewBag.Ucenik = ocjena.Ucenik;
            ViewBag.Predmet = ocjena.Predmet;

            return View(ocjena);
        }

        [HttpPost]
        public async Task<IActionResult> Uredi(Ocjena o)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Ucenik = await _userManager.FindByIdAsync(o.UcenikId);
                ViewBag.Predmet = await _context.Predmet.FindAsync(o.PredmetId);
                return View(o);
            }

            var postojeca = await _context.Ocjena
                .Include(x => x.Ucenik)
                .FirstOrDefaultAsync(x => x.Id == o.Id);

            if (postojeca == null)
                return NotFound();

            postojeca.Vrijednost = o.Vrijednost;
            postojeca.Komentar = o.Komentar;

            await _context.SaveChangesAsync();
            return RedirectToAction("Rezime", new { razredId = postojeca.Ucenik.RazredId, predmetId = postojeca.PredmetId });
        }




        [HttpPost]
        public async Task<IActionResult> Obrisi(int id)
        {
            var ocjena = await _context.Ocjena.Include(o => o.Ucenik).FirstOrDefaultAsync(o => o.Id == id);
            if (ocjena == null) return NotFound();

            int predmetId = ocjena.PredmetId;
            int? razredId = ocjena.Ucenik.RazredId;

            _context.Ocjena.Remove(ocjena);
            await _context.SaveChangesAsync();

            return RedirectToAction("Rezime", new { razredId = razredId, predmetId = predmetId });
        }
    }
}
