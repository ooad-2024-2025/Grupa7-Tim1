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
    public class RazredController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RazredController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Razreds
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Razred.Include(r => r.Nastavnik);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Razreds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var razred = await _context.Razred
                .Include(r => r.Nastavnik)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (razred == null)
            {
                return NotFound();
            }

            return View(razred);
        }

        // GET: Razreds/Create
        public IActionResult Create()
        {
            ViewData["NastavnikId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Razreds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Naziv,NastavnikId")] Razred razred)
        {
            if (ModelState.IsValid)
            {
                _context.Add(razred);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["NastavnikId"] = new SelectList(_context.Users, "Id", "Id", razred.NastavnikId);
            return View(razred);
        }

        // GET: Razreds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var razred = await _context.Razred.FindAsync(id);
            if (razred == null)
            {
                return NotFound();
            }
            ViewData["NastavnikId"] = new SelectList(_context.Users, "Id", "Id", razred.NastavnikId);
            return View(razred);
        }

        // POST: Razreds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Naziv,NastavnikId")] Razred razred)
        {
            if (id != razred.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(razred);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RazredExists(razred.Id))
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
            ViewData["NastavnikId"] = new SelectList(_context.Users, "Id", "Id", razred.NastavnikId);
            return View(razred);
        }

        // GET: Razreds/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var razred = await _context.Razred
                .Include(r => r.Nastavnik)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (razred == null)
            {
                return NotFound();
            }

            return View(razred);
        }

        // POST: Razreds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var razred = await _context.Razred.FindAsync(id);
            if (razred != null)
            {
                _context.Razred.Remove(razred);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RazredExists(int id)
        {
            return _context.Razred.Any(e => e.Id == id);
        }
    }
}
