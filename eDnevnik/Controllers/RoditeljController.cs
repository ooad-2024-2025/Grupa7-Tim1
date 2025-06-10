using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eDnevnik.Models;
using eDnevnik.Data;
using eDnevnik.ViewModels;

namespace eDnevnik.Controllers
{
    [Authorize(Roles = "Roditelj")]
    public class RoditeljController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ApplicationDbContext _context;

        public RoditeljController(UserManager<Korisnik> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> OcjeneDjeteta()
        {
            var roditelj = await _userManager.GetUserAsync(User);

            if (roditelj == null)
                return Unauthorized("Niste prijavljeni.");

            var ucenik = await _context.Users
                .Include(u => u.Razred)
                .FirstOrDefaultAsync(u => u.RoditeljId == roditelj.Id);

            if (ucenik == null)
                return NotFound("Dijete nije pronađeno.");

            var ocjene = await _context.Ocjena
                .Include(o => o.Predmet)
                .Where(o => o.UcenikId == ucenik.Id)
                .ToListAsync();

            var viewModel = new UcenikOcjeneViewModel
            {
                Ucenik = ucenik,
                OcjenePoPremetima = ocjene
                    .GroupBy(o => o.Predmet)
                    .Select(g => new OcjenaPoPredmetu
                    {
                        Predmet = g.Key,
                        Ocjene = g.ToList(),
                        BrojOcjena = g.Count(),
                        Prosjek = g.Average(x => x.Vrijednost),
                        NajnovijaOcjena = g.OrderByDescending(x => x.Datum).FirstOrDefault()
                    }).ToList(),
                Statistike = new UcenikStatistike
                {
                    OpciProsjek = ocjene.Any() ? ocjene.Average(x => x.Vrijednost) : 0,
                    BrojPredmeta = ocjene.Select(x => x.PredmetId).Distinct().Count(),
                    UkupnoOcjena = ocjene.Count()
                }
            };

            return View(viewModel);
        }
    }
}
