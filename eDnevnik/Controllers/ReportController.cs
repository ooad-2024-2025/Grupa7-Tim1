using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using eDnevnik.Models;
using eDnevnik.Services;
using eDnevnik.Data;
using Microsoft.EntityFrameworkCore;

namespace eDnevnik.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ExcelReportService _reportService;
        private readonly EmailService _emailService;
        private readonly ApplicationDbContext _dbContext;

        public ReportController(
            UserManager<Korisnik> userManager,
            ExcelReportService reportService,
            EmailService emailService,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _reportService = reportService;
            _emailService = emailService;
            _dbContext = dbContext;
        }

        // POSTOJEĆA METODA - ostavi kako jeste
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string? ucenikId)
        {
            var prijavljeni = await _userManager.GetUserAsync(User);
            Korisnik ciljani;

            // Dohvati role
            var roleList = await _userManager.GetRolesAsync(prijavljeni);
            var isAdmin = roleList.Contains("Administrator");
            var isNastavnik = roleList.Contains("Nastavnik") || roleList.Contains("Profesor");
            var isUcenik = roleList.Contains("Ucenik");
            var isRoditelj = roleList.Contains("Roditelj");

            // ADMIN / NASTAVNIK šalju za učenika, ali dobijaju izvještaj na svoj e-mail
            if (!string.IsNullOrEmpty(ucenikId) && (isAdmin || isNastavnik))
            {
                ciljani = await _userManager.FindByIdAsync(ucenikId);
            }
            // UČENIK sam sebi
            else if (isUcenik)
            {
                ciljani = prijavljeni;
            }
            // RODITELJ šalje za svoje dijete
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

            // 📬 kome se šalje mail?
            var toEmail = (isAdmin || isNastavnik) ? prijavljeni.Email : ciljani.Email;

            await _emailService.SendEmailAsync(
                toEmail: toEmail,
                subject: $"Izvještaj za učenika {ciljani.Ime} {ciljani.Prezime}",
                body: $"U prilogu se nalazi izvještaj za učenika {ciljani.Ime} {ciljani.Prezime}.",
                attachment: reportBytes,
                filename: "izvjestaj.xlsx"
            );

            return RedirectToAction("Potvrda");
        }

        // POSTOJEĆA METODA - ostavi kako jeste
        [HttpPost]
        [Authorize(Roles = "Roditelj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToParent(string ucenikId)
        {
            var roditelj = await _userManager.GetUserAsync(User);
            if (roditelj == null)
                return Unauthorized();

            var ucenik = await _userManager.Users
                .Include(u => u.Razred)
                .FirstOrDefaultAsync(u => u.Id == ucenikId && u.RoditeljId == roditelj.Id);

            if (ucenik == null)
                return NotFound("Dijete nije pronađeno.");

            var report = await _reportService.GenerateReportAsync(ucenik.Id);
            if (report == null)
                return BadRequest("Greška pri generisanju izvještaja.");

            await _emailService.SendEmailAsync(
                toEmail: roditelj.Email,
                subject: $"Izvještaj za dijete {ucenik.Ime} {ucenik.Prezime}",
                body: $"Poštovani, u prilogu se nalazi izvještaj za Vaše dijete {ucenik.Ime} {ucenik.Prezime}.",
                attachment: report,
                filename: $"Izvjestaj_{ucenik.Ime}_{ucenik.Prezime}.xlsx"
            );

            return RedirectToAction("Potvrda");
        }

        // TEST GENERATE SA HARDKODIRANIM ID
        [HttpGet]
        public async Task<IActionResult> TestGenerate()
        {
            try
            {
                Console.WriteLine("TestGenerate called");

                var trenutniKorisnik = await _userManager.GetUserAsync(User);
                Console.WriteLine($"Current user: {trenutniKorisnik?.FullName}");

                // Uzmi bilo koje dijete ovog roditelja
                var ucenik = await _dbContext.Users
                    .Include(u => u.Razred)
                    .Where(u => u.RoditeljId == trenutniKorisnik.Id)
                    .FirstOrDefaultAsync();

                if (ucenik == null)
                {
                    return Content("Nema pronađene djece za ovog roditelja");
                }

                Console.WriteLine($"Found ucenik: {ucenik.FullName}");

                // Pokušaj generirati PDF
                var pdfBytes = await _reportService.GeneratePdfReportAsync(ucenik.Id);
                Console.WriteLine($"PDF generated successfully, size: {pdfBytes.Length}");

                return File(pdfBytes, "application/pdf", "test-generate.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestGenerate error: {ex.Message}");
                return Content($"Greška: {ex.Message}");
            }
        }

        // TEST PDF METODA
        [HttpGet]
        public IActionResult TestPdf()
        {
            var testBytes = System.Text.Encoding.UTF8.GetBytes("Test PDF content");
            return File(testBytes, "application/pdf", "test.pdf");
        }

        // TEST METODA ZA PROVJERU
        [HttpGet]
        public IActionResult Test()
        {
            return Content("Report controller works!");
        }

        // ALTERNATIVNA METODA SA DRUGAČIJIM PARAMETROM
        [HttpGet]
        public async Task<IActionResult> GenerateReport(string dijeteId)
        {
            try
            {
                Console.WriteLine($"GenerateReport called with dijeteId: '{dijeteId}'");

                if (string.IsNullOrEmpty(dijeteId))
                {
                    Console.WriteLine("DijeteId is null or empty");
                    return BadRequest("ID djeteta nije valjan");
                }

                var trenutniKorisnik = await _userManager.GetUserAsync(User);
                if (trenutniKorisnik == null)
                {
                    return Unauthorized("Korisnik nije autentificiran");
                }

                var roles = await _userManager.GetRolesAsync(trenutniKorisnik);
                Console.WriteLine($"User roles: {string.Join(", ", roles)}");

                // Provjeri dozvole
                Korisnik ucenik = null;
                if (roles.Contains("Roditelj"))
                {
                    ucenik = await _dbContext.Users
                        .Include(u => u.Razred)
                        .FirstOrDefaultAsync(u => u.Id == dijeteId && u.RoditeljId == trenutniKorisnik.Id);
                }

                if (ucenik == null)
                {
                    Console.WriteLine("Ucenik not found or access denied");
                    return NotFound("Učenik nije pronađen ili nemate dozvolu pristupa.");
                }

                var pdfBytes = await _reportService.GeneratePdfReportAsync(ucenik.Id);
                var fileName = $"Izvještaj_{ucenik.Ime}_{ucenik.Prezime}_{DateTime.Now:yyyyMMdd}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GenerateReport error: {ex.Message}");
                return BadRequest($"Greška: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Generate(string id)
        {
            try
            {
                Console.WriteLine($"Generate called with id parameter: '{id}'");
                Console.WriteLine($"Request.Query keys: {string.Join(", ", Request.Query.Keys)}");
                Console.WriteLine($"Request.RouteValues: {string.Join(", ", Request.RouteValues.Select(kv => $"{kv.Key}={kv.Value}"))}");

                // Provjeri da li je id null ili prazan
                if (string.IsNullOrEmpty(id))
                {
                    Console.WriteLine("Id is null or empty");

                    // Pokušaj dohvatiti iz route values
                    if (Request.RouteValues.ContainsKey("id"))
                    {
                        id = Request.RouteValues["id"]?.ToString();
                        Console.WriteLine($"Got id from RouteValues: '{id}'");
                    }

                    if (string.IsNullOrEmpty(id))
                    {
                        Console.WriteLine("Still no id found");
                        return BadRequest("ID djeteta nije valjan - parametar nije prosliješen");
                    }
                }

                var trenutniKorisnik = await _userManager.GetUserAsync(User);
                if (trenutniKorisnik == null)
                {
                    Console.WriteLine("Current user is null");
                    return Unauthorized("Korisnik nije autentificiran");
                }

                Console.WriteLine($"Current user: {trenutniKorisnik?.FullName}");

                var roles = await _userManager.GetRolesAsync(trenutniKorisnik);
                Console.WriteLine($"User roles: {string.Join(", ", roles)}");

                // Provjeri dozvole
                Korisnik ucenik = null;
                if (roles.Contains("Roditelj"))
                {
                    ucenik = await _dbContext.Users
                        .Include(u => u.Razred)
                        .FirstOrDefaultAsync(u => u.Id == id && u.RoditeljId == trenutniKorisnik.Id);
                    Console.WriteLine($"Found ucenik for parent: {ucenik?.FullName}");
                }
                else if (roles.Contains("Ucenik"))
                {
                    ucenik = await _dbContext.Users
                        .Include(u => u.Razred)
                        .FirstOrDefaultAsync(u => u.Id == trenutniKorisnik.Id);
                    Console.WriteLine($"Ucenik accessing own report: {ucenik?.FullName}");
                }
                else if (roles.Contains("Nastavnik") || roles.Contains("Administrator"))
                {
                    ucenik = await _dbContext.Users
                        .Include(u => u.Razred)
                        .FirstOrDefaultAsync(u => u.Id == id);
                    Console.WriteLine($"Admin/Teacher accessing report for: {ucenik?.FullName}");
                }

                if (ucenik == null)
                {
                    Console.WriteLine("Ucenik not found or access denied");
                    return NotFound("Učenik nije pronađen ili nemate dozvolu pristupa.");
                }

                Console.WriteLine($"Generating PDF for: {ucenik.FullName}");

                // Provjeri da li postoji GeneratePdfReportAsync metoda
                try
                {
                    var pdfBytes = await _reportService.GeneratePdfReportAsync(ucenik.Id);
                    Console.WriteLine($"PDF generated, size: {pdfBytes.Length} bytes");

                    if (pdfBytes == null || pdfBytes.Length == 0)
                    {
                        Console.WriteLine("PDF bytes are null or empty");
                        return BadRequest("Generiranje PDF-a nije uspjelo");
                    }

                    var fileName = $"Izvještaj_{ucenik.Ime}_{ucenik.Prezime}_{DateTime.Now:yyyyMMdd}.pdf";
                    Console.WriteLine($"Returning file: {fileName}");

                    // Explicit headers za download
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(pdfBytes, "application/pdf", fileName);
                }
                catch (Exception pdfEx)
                {
                    Console.WriteLine($"PDF generation error: {pdfEx.Message}");
                    return BadRequest($"Greška pri generiranju PDF-a: {pdfEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return BadRequest($"Greška: {ex.Message}");
            }
        }

        // NOVA METODA ZA SLANJE PDF-a NA EMAIL
        [HttpPost]
        [Authorize(Roles = "Roditelj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendPdfToParent(string ucenikId)
        {
            try
            {
                var roditelj = await _userManager.GetUserAsync(User);
                var ucenik = await _dbContext.Users
                    .Include(u => u.Razred)
                    .FirstOrDefaultAsync(u => u.Id == ucenikId && u.RoditeljId == roditelj.Id);

                if (ucenik == null)
                {
                    TempData["ErrorMessage"] = "Učenik nije pronađen.";
                    return RedirectToAction("Index", "Roditelj");
                }

                if (string.IsNullOrEmpty(roditelj.Email))
                {
                    TempData["ErrorMessage"] = "Nemate podešenu email adresu.";
                    return RedirectToAction("Index", "Roditelj");
                }

                // Generiši PDF izvještaj
                var pdfBytes = await _reportService.GeneratePdfReportAsync(ucenik.Id);
                var fileName = $"Izvještaj_{ucenik.Ime}_{ucenik.Prezime}_{DateTime.Now:yyyyMMdd}.pdf";

                // Pošalji email
                var subject = $"eDnevnik - PDF Izvještaj za {ucenik.FullName}";
                var body = _emailService.KreirajHtmlSadržaj(
                    $"PDF Izvještaj za {ucenik.FullName}",
                    $"Poštovani {roditelj.FullName},<br><br>" +
                    $"U prilogu se nalazi PDF izvještaj o napretku vašeg djeteta {ucenik.FullName} iz {ucenik.Razred?.Naziv ?? "nepoznatog razreda"}.<br><br>" +
                    $"PDF izvještaj sadrži detaljni pregled ocjena, izostanaka i vladanja.",
                    $"<p><strong>Datum generiranja:</strong> {DateTime.Now:dd.MM.yyyy u HH:mm}</p>"
                );

                await _emailService.SendEmailAsync(roditelj.Email, subject, body, pdfBytes, fileName);

                TempData["SuccessMessage"] = $"PDF izvještaj je uspješno poslan na {roditelj.Email}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Greška prilikom slanja PDF izvještaja: " + ex.Message;
            }

            return RedirectToAction("OcjeneDjeteta", "Roditelj", new { dijeteId = ucenikId });
        }

        // POSTOJEĆA METODA - ostavi kako jeste
        private bool RoditeljJePovezanSaUcenikom(string roditeljId, string ucenikId)
        {
            return _dbContext.Users.Any(u =>
                u.Id == ucenikId &&
                u.RoditeljId == roditeljId);
        }

        // POSTOJEĆE METODE - ostavi kako jesu
        public IActionResult Potvrda() => View();

        public IActionResult Greska()
        {
            ViewBag.Message = "Niste ovlašteni za pregled izvještaja.";
            return View();
        }
    }
}