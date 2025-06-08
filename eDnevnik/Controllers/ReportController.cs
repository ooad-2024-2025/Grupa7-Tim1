using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using eDnevnik.Models;
using eDnevnik.Services;
using eDnevnik.Data; // ako ti je tu DbContext

namespace eDnevnik.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ExcelReportService _reportService;
        private readonly EmailService _emailService;
        private readonly ApplicationDbContext _dbContext; // dodaj ako već nemaš

        public ReportController(
            UserManager<Korisnik> userManager,
            ExcelReportService reportService,
            EmailService emailService,
            ApplicationDbContext dbContext) // ubaciti u konstruktor
        {
            _userManager = userManager;
            _reportService = reportService;
            _emailService = emailService;
            _dbContext = dbContext;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string? ucenikId)
        {
            var prijavljeni = await _userManager.GetUserAsync(User);
            Korisnik ciljani;

            // Dohvati sve role korisnika
            var roleList = await _userManager.GetRolesAsync(prijavljeni);
            var isAdmin = roleList.Contains("Administrator");
            var isNastavnik = roleList.Contains("Nastavnik") || roleList.Contains("Profesor");
            var isUcenik = roleList.Contains("Ucenik");
            var isRoditelj = roleList.Contains("Roditelj");

            // Admin ili Nastavnik mogu slati za bilo kojeg učenika
            if (!string.IsNullOrEmpty(ucenikId) && (isAdmin || isNastavnik))
            {
                ciljani = await _userManager.FindByIdAsync(ucenikId);
            }
            // Učenik može sebi
            else if (isUcenik)
            {
                ciljani = prijavljeni;
            }
            // Roditelj može za svoje dijete
            else if (isRoditelj)
            {
                ciljani = await _userManager.FindByIdAsync(ucenikId);
                if (ciljani == null || !RoditeljJePovezanSaUcenikom(prijavljeni.Id, ciljani.Id))
                {
                    return RedirectToAction("Greska");
                }
            }
            else
            {
                return RedirectToAction("Greska");
            }

            if (ciljani == null || string.IsNullOrWhiteSpace(ciljani.Email))
                return RedirectToAction("Greska");

            var reportBytes = await _reportService.GenerateReportAsync(ciljani.Id);

            await _emailService.SendEmailAsync(
                toEmail: ciljani.Email,
                subject: "Vaš izvještaj o učeniku",
                body: "U prilogu se nalazi generisani izvještaj o ocjenama, izostancima i ponašanju.",
                attachment: reportBytes,
                filename: "izvjestaj.xlsx"
            );

            return RedirectToAction("Potvrda");
        }


        // ✅ OVDE DODAJEŠ METODU:
        private bool RoditeljJePovezanSaUcenikom(string roditeljId, string ucenikId)
        {
            return _dbContext.Users.Any(u =>
                u.Id == ucenikId &&
                u.RoditeljId == roditeljId);
        }

        public IActionResult Potvrda() => View();
        public IActionResult Greska()
        {
            ViewBag.Message = "Niste ovlašteni za pregled izvještaja.";
            return View();
        }
    }
}