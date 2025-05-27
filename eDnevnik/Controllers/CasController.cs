using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using eDnevnik.Data;
using eDnevnik.Models;

namespace eDnevnik.Controllers
{
    public class CasController : Controller
    {
        //potrebno da bi mogao da radi sa bazom, controller je povezan sa cas klasom modela
        private readonly ApplicationDbContext _context;

        //konstruktor potreban da bi sistem koji vodi brigu o zivotnom ciklusu mogao instancirati controller
        public CasController(ApplicationDbContext context)
        {
            _context = context;
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
    }
}
