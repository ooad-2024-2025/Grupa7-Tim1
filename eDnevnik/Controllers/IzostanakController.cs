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
    public class IzostanakController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IzostanakController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Izostanak
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Izostanak.Include(i => i.Cas).Include(i => i.Ucenik);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Izostanak/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var izostanak = await _context.Izostanak
                .Include(i => i.Cas)
                .Include(i => i.Ucenik)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (izostanak == null)
            {
                return NotFound();
            }

            return View(izostanak);
        }

        // GET: Izostanak/Create
        public IActionResult Create()
        {
            ViewData["CasId"] = new SelectList(_context.Cas, "Id", "Id");
            ViewData["UcenikId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Izostanak/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Komentar,status,UcenikId,CasId")] Izostanak izostanak)
        {
            if (ModelState.IsValid)
            {
                _context.Add(izostanak);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CasId"] = new SelectList(_context.Cas, "Id", "Id", izostanak.CasId);
            ViewData["UcenikId"] = new SelectList(_context.Users, "Id", "Id", izostanak.UcenikId);
            return View(izostanak);
        }

        // GET: Izostanak/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var izostanak = await _context.Izostanak.FindAsync(id);
            if (izostanak == null)
            {
                return NotFound();
            }
            ViewData["CasId"] = new SelectList(_context.Cas, "Id", "Id", izostanak.CasId);
            ViewData["UcenikId"] = new SelectList(_context.Users, "Id", "Id", izostanak.UcenikId);
            return View(izostanak);
        }

        // POST: Izostanak/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Komentar,status,UcenikId,CasId")] Izostanak izostanak)
        {
            if (id != izostanak.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(izostanak);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IzostanakExists(izostanak.Id))
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
            ViewData["CasId"] = new SelectList(_context.Cas, "Id", "Id", izostanak.CasId);
            ViewData["UcenikId"] = new SelectList(_context.Users, "Id", "Id", izostanak.UcenikId);
            return View(izostanak);
        }

        // GET: Izostanak/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var izostanak = await _context.Izostanak
                .Include(i => i.Cas)
                .Include(i => i.Ucenik)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (izostanak == null)
            {
                return NotFound();
            }

            return View(izostanak);
        }

        // POST: Izostanak/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var izostanak = await _context.Izostanak.FindAsync(id);
            if (izostanak != null)
            {
                _context.Izostanak.Remove(izostanak);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IzostanakExists(int id)
        {
            return _context.Izostanak.Any(e => e.Id == id);
        }
    }
}
