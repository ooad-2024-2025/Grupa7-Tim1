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
    public class OcjenaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OcjenaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ocjena
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Ocjena.Include(o => o.Predmet).Include(o => o.Ucenik);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Ocjena/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ocjena = await _context.Ocjena
                .Include(o => o.Predmet)
                .Include(o => o.Ucenik)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ocjena == null)
            {
                return NotFound();
            }

            return View(ocjena);
        }

        // GET: Ocjena/Create
        public IActionResult Create()
        {
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Id");
            ViewData["UcenikId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Ocjena/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UcenikId,PredmetId,Vrijednost,Komentar,Datum")] Ocjena ocjena)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ocjena);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Id", ocjena.PredmetId);
            ViewData["UcenikId"] = new SelectList(_context.Users, "Id", "Id", ocjena.UcenikId);
            return View(ocjena);
        }

        // GET: Ocjena/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ocjena = await _context.Ocjena.FindAsync(id);
            if (ocjena == null)
            {
                return NotFound();
            }
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Id", ocjena.PredmetId);
            ViewData["UcenikId"] = new SelectList(_context.Users, "Id", "Id", ocjena.UcenikId);
            return View(ocjena);
        }

        // POST: Ocjena/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UcenikId,PredmetId,Vrijednost,Komentar,Datum")] Ocjena ocjena)
        {
            if (id != ocjena.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ocjena);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OcjenaExists(ocjena.Id))
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
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Id", ocjena.PredmetId);
            ViewData["UcenikId"] = new SelectList(_context.Users, "Id", "Id", ocjena.UcenikId);
            return View(ocjena);
        }

        // GET: Ocjena/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ocjena = await _context.Ocjena
                .Include(o => o.Predmet)
                .Include(o => o.Ucenik)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ocjena == null)
            {
                return NotFound();
            }

            return View(ocjena);
        }

        // POST: Ocjena/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ocjena = await _context.Ocjena.FindAsync(id);
            if (ocjena != null)
            {
                _context.Ocjena.Remove(ocjena);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OcjenaExists(int id)
        {
            return _context.Ocjena.Any(e => e.Id == id);
        }
    }
}
