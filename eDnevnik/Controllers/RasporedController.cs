using eDnevnik.Data;
using eDnevnik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Ensure System.Linq is imported

namespace eDnevnik.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class RasporedController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RasporedController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var casovi = await _context.Cas
                .Include(c => c.Razred)
                .Include(c => c.Predmet)
                .Include(c => c.Nastavnik)
                .ToListAsync();

            return View(casovi);
        }

        public IActionResult Create()
        {
            ViewData["RazredId"] = new SelectList(_context.Razred, "Id", "Naziv");
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Naziv");
            ViewData["NastavnikId"] = new SelectList(
                _context.Users.Select(u => new { u.Id, ImePrezime = u.Ime + " " + u.Prezime }),
                "Id", "ImePrezime"
            );
            return View(new Cas());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cas cas)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cas);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is not valid, repopulate ViewData for dropdowns consistently
            // and return the view with validation errors.
            // You can add logging here to see the ModelState errors as shown in the explanation.
            
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                System.Diagnostics.Debug.WriteLine($"ModelState Error: {error.ErrorMessage}");
                if (error.Exception != null)
                {
                    System.Diagnostics.Debug.WriteLine($"ModelState Exception: {error.Exception.Message}");
                }
            }
            

            ViewData["RazredId"] = new SelectList(_context.Razred, "Id", "Naziv", cas.RazredId);
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Naziv", cas.PredmetId);
            // Corrected way to populate NastavnikId SelectList
            ViewData["NastavnikId"] = new SelectList(
                _context.Users.Select(u => new { u.Id, ImePrezime = u.Ime + " " + u.Prezime }),
                "Id", "ImePrezime", cas.NastavnikId
            );
            return View(cas);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cas = await _context.Cas.FindAsync(id);
            if (cas == null) return NotFound();

            ViewData["RazredId"] = new SelectList(_context.Razred, "Id", "Naziv", cas.RazredId);
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Naziv", cas.PredmetId);
            // Consistent population for Edit GET as well
            ViewData["NastavnikId"] = new SelectList(
                 _context.Users.Select(u => new { u.Id, ImePrezime = u.Ime + " " + u.Prezime }),
                "Id", "ImePrezime", cas.NastavnikId
            );
            return View(cas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cas cas)
        {
            if (id != cas.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cas);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CasExists(cas.Id)) // Assuming you have a CasExists method
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
            // Repopulate ViewData if ModelState is invalid
            ViewData["RazredId"] = new SelectList(_context.Razred, "Id", "Naziv", cas.RazredId);
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Naziv", cas.PredmetId);
            ViewData["NastavnikId"] = new SelectList(
                 _context.Users.Select(u => new { u.Id, ImePrezime = u.Ime + " " + u.Prezime }),
                "Id", "ImePrezime", cas.NastavnikId
            );
            return View(cas);
        }

        // Helper method (optional, but good practice for Edit)
        private bool CasExists(int id)
        {
            return _context.Cas.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var cas = await _context.Cas
                .Include(c => c.Predmet)
                .Include(c => c.Razred)
                .Include(c => c.Nastavnik)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cas == null) return NotFound();

            return View(cas);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cas = await _context.Cas.FindAsync(id);
            if (cas != null)
            {
                _context.Cas.Remove(cas);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}