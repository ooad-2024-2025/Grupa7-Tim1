using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using eDnevnik.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace eDnevnik.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<Korisnik> _userManager;

    public HomeController(ILogger<HomeController> logger, UserManager<Korisnik> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Administrator"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (roles.Contains("Ucenik"))
            {
                return RedirectToAction("Index", "Ucenik");
            }
            else if (roles.Contains("Nastavnik"))
            {
                return RedirectToAction("Index", "Evidencija");
            }
        }

        return View(); // za neulogovane i ostale korisnike
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
