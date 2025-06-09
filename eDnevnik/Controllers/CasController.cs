using eDnevnik.Data;
using eDnevnik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eDnevnik.Controllers
{
    public class CasController : Controller
    {
        //potrebno da bi mogao da radi sa bazom, controller je povezan sa cas klasom modela
        private readonly ApplicationDbContext _context;

        // Add the following private field to the CasController class OVO JE SAMO ZA TESTIRANJE
        private readonly UserManager<Korisnik> _userManager;

        //konstruktor potreban da bi sistem koji vodi brigu o zivotnom ciklusu mogao instancirati controller
        //OVO URADIO SAMO ZBOG TESTIRANJA
        //public CasController(ApplicationDbContext context)
        //{
        //    _context = context;
        //}
        //PRIJE JE OVDJE IDENTITYUSER UMJESTO KROSNIKA
        // Update the constructor to initialize the _userManager field I OVO ZA TESTIRANJE
        public CasController(ApplicationDbContext context, UserManager<Korisnik> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cas
        //prikaz svih casova, linq metode ukljucuju i ostale entitete
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Cas.Include(c => c.Nastavnik).Include(c => c.Predmet).Include(c => c.Razred);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Cas/Details/5
        //Details/5 prikazuje detalje za poseban slog u tabeli Ocjena koji ima id 5 recimo 
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cas = await _context.Cas
                .Include(c => c.Nastavnik)
                .Include(c => c.Predmet)
                .Include(c => c.Razred)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cas == null)
            {
                return NotFound();
            }

            return View(cas);
        }

        // GET: Cas/Create
        //prikaz forme za popunjavanje za unos casa, selectlist pokazuje stvari-drop-down liste iz kojih mozemo izabrati odredjene podatke
        public IActionResult Create()
        {
            ViewData["NastavnikId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Id");
            ViewData["RazredId"] = new SelectList(_context.Razred, "Id", "Id");
            return View();
        }

        // POST: Cas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Termin,RazredId,PredmetId,NastavnikId")] Cas cas)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cas);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["NastavnikId"] = new SelectList(_context.Users, "Id", "Id", cas.NastavnikId);
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Id", cas.PredmetId);
            ViewData["RazredId"] = new SelectList(_context.Razred, "Id", "Id", cas.RazredId);
            return View(cas);
        }

        // GET: Cas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cas = await _context.Cas.FindAsync(id);
            if (cas == null)
            {
                return NotFound();
            }
            ViewData["NastavnikId"] = new SelectList(_context.Users, "Id", "Id", cas.NastavnikId);
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Id", cas.PredmetId);
            ViewData["RazredId"] = new SelectList(_context.Razred, "Id", "Id", cas.RazredId);
            return View(cas);
        }

        // POST: Cas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Termin,RazredId,PredmetId,NastavnikId")] Cas cas)
        {
            if (id != cas.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cas);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["NastavnikId"] = new SelectList(_context.Users, "Id", "Id", cas.NastavnikId);
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Id", cas.PredmetId);
            ViewData["RazredId"] = new SelectList(_context.Razred, "Id", "Id", cas.RazredId);
            return View(cas);
        }

        // GET: Cas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cas = await _context.Cas
                .Include(c => c.Nastavnik)
                .Include(c => c.Predmet)
                .Include(c => c.Razred)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cas == null)
            {
                return NotFound();
            }

            return View(cas);
        }

        // POST: Cas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cas = await _context.Cas.FindAsync(id);
            if (cas != null)
            {
                _context.Cas.Remove(cas);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CasExists(int id)
        {
            return _context.Cas.Any(e => e.Id == id);
        }
        //BEZZE PODATKE DODAO SAM
        public async Task<IActionResult> DodajTestCasove()
        {
            var predmet = await _context.Predmet.FirstOrDefaultAsync();
            var razred = await _context.Razred.FirstOrDefaultAsync();
            var nastavnik = await _userManager.Users.FirstOrDefaultAsync(u => u.Email.Contains("nastavnik"));

            if (predmet == null || razred == null || nastavnik == null)
                return Content("Nedostaju entiteti za test unos.");

            var termini = new[]
            {
        new DateTime(2025, 6, 10, 8, 0, 0), // ponedjeljak
        new DateTime(2025, 6, 11, 8, 50, 0), // utorak
        new DateTime(2025, 6, 12, 9, 40, 0), // srijeda
    };

            foreach (var termin in termini)
            {
                _context.Cas.Add(new Cas
                {
                    Termin = termin,
                    PredmetId = predmet.Id,
                    RazredId = razred.Id,
                    NastavnikId = nastavnik.Id
                });
            }

            await _context.SaveChangesAsync();
            return Content("Test časovi dodani.");
        }


        [Authorize]
        public async Task<IActionResult> SedmicniRaspored(string dan, string nastavnikId, int? predmetId, int? razredId)
        {
            var casovi = await _context.Cas
                .Include(c => c.Predmet)
                .Include(c => c.Razred)
                .Include(c => c.Nastavnik)
                .ToListAsync();

            if (!string.IsNullOrEmpty(dan))
            {
                var danFilter = Enum.Parse<DayOfWeek>(dan);
                casovi = casovi.Where(c => c.Termin.DayOfWeek == danFilter).ToList();
                ViewBag.IzabraniDan = dan;
            }

            if (!string.IsNullOrEmpty(nastavnikId))
            {
                casovi = casovi.Where(c => c.NastavnikId == nastavnikId).ToList();
                ViewBag.IzabraniNastavnik = nastavnikId;
            }

            if (predmetId.HasValue)
            {
                casovi = casovi.Where(c => c.PredmetId == predmetId).ToList();
                ViewBag.IzabraniPredmet = predmetId;
            }

            if (razredId.HasValue)
            {
                casovi = casovi.Where(c => c.RazredId == razredId).ToList();
                ViewBag.IzabraniRazred = razredId;
            }




            var sviKorisnici = await _userManager.Users.ToListAsync();
            var samoNastavnici = new List<Korisnik>();

            foreach (var korisnik in sviKorisnici)
            {
                if (await _userManager.IsInRoleAsync(korisnik, "Nastavnik"))
                {
                    samoNastavnici.Add(korisnik);
                }
            }
            ViewBag.Nastavnici = new SelectList(samoNastavnici, "Id", "FullName");

            ViewBag.Predmeti = new SelectList(_context.Predmet, "Id", "Naziv");

            ViewBag.Razredi = new SelectList(_context.Razred, "Id", "Naziv");

            return View(casovi);
        }




    }
}
