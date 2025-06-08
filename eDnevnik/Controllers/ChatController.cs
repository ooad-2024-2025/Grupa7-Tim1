using eDnevnik.Data;
using eDnevnik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eDnevnik.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace eDnevnik.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;
        private readonly IHubContext<ChatHub> _hub;

        public ChatController(ApplicationDbContext context, UserManager<Korisnik> userManager, IHubContext<ChatHub> hub)
        {
            _context = context;
            _userManager = userManager;
            _hub = hub;
        }


        public async Task<IActionResult> Index()
        {
            var poruke = await _context.Poruka
                .Include(p => p.Posiljalac)
                .OrderBy(p => p.VrijemeSlanja)
                .ToListAsync();

            return View(poruke);
        }

        [HttpPost]
        public async Task<IActionResult> Posalji(string sadrzaj)
        {
            var posiljalac = await _userManager.GetUserAsync(User);

            var poruka = new Poruka
            {
                PosiljalacId = posiljalac.Id,
                PrimalacId = null, // jer je javna
                Sadrzaj = sadrzaj
            };

            _context.Poruka.Add(poruka);
            await _context.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("PrimiPoruku",
                posiljalac.FullName,
                sadrzaj,
                DateTime.Now.ToString("HH:mm"));

            return RedirectToAction("Index");
        }

    }
}
