using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eDnevnik.Data;
using eDnevnik.Models;

namespace eDnevnik.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class PredmetiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;

        public PredmetiController(ApplicationDbContext context, UserManager<Korisnik> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var predmeti = _context.Predmet
                .Include(p => p.Nastavnik)
                .ToList();

            return View(predmeti);
        }

        [HttpGet]
        public async Task<IActionResult> Dodaj()
        {
            var nastavnici = await _userManager.GetUsersInRoleAsync("Nastavnik");
            ViewBag.Nastavnici = nastavnici;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Dodaj(Predmet predmet)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Nastavnici = await _userManager.GetUsersInRoleAsync("Nastavnik");
                return View(predmet);
            }

            _context.Predmet.Add(predmet);
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = $"Predmet '{predmet.Naziv}' je uspješno dodan.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var predmet = await _context.Predmet.FindAsync(id);
            if (predmet == null)
                return NotFound();

            ViewBag.Nastavnici = await _userManager.GetUsersInRoleAsync("Nastavnik");
            return View(predmet);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Predmet predmet)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Nastavnici = await _userManager.GetUsersInRoleAsync("Nastavnik");
                return View(predmet);
            }

            _context.Predmet.Update(predmet);
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = $"Predmet '{predmet.Naziv}' je uspješno ažuriran.";
            return RedirectToAction("Index");
        }

        // NOVA METODA - Prikaži podatke o brisanju prije potvrde
        [HttpGet]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            var predmet = await _context.Predmet
                .Include(p => p.Nastavnik)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (predmet == null)
            {
                TempData["Greska"] = "Predmet nije pronađen.";
                return RedirectToAction("Index");
            }

            // Dohvati statistike vezanih podataka
            var brojCasova = await _context.Cas
                .CountAsync(c => c.PredmetId == id);

            var brojOcjena = await _context.Ocjena
                .CountAsync(o => o.PredmetId == id);

            var brojIzostanaka = await _context.Izostanak
                .Include(i => i.Cas)
                .CountAsync(i => i.Cas.PredmetId == id);

            var brojEvidencija = await _context.EvidencijaCasa
                .Include(e => e.Cas)
                .CountAsync(e => e.Cas.PredmetId == id);

            ViewBag.BrojCasova = brojCasova;
            ViewBag.BrojOcjena = brojOcjena;
            ViewBag.BrojIzostanaka = brojIzostanaka;
            ViewBag.BrojEvidencija = brojEvidencija;

            return View(predmet);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var predmet = await _context.Predmet.FindAsync(id);
                if (predmet == null)
                {
                    TempData["Greska"] = "Predmet nije pronađen.";
                    return RedirectToAction("Index");
                }

                string predmetNaziv = predmet.Naziv;

                // Statistike za log
                var brojOcjena = await _context.Ocjena.CountAsync(o => o.PredmetId == id);
                var brojCasova = await _context.Cas.CountAsync(c => c.PredmetId == id);
                var brojIzostanaka = await _context.Izostanak
                    .Include(i => i.Cas)
                    .CountAsync(i => i.Cas.PredmetId == id);

                // POKUŠAJ 1: Ručno brisanje u pravilnom redoslijedu
                try
                {
                    // 1. Obrisi sve izostanke povezane sa časovima ovog predmeta
                    var izostanci = await _context.Izostanak
                        .Include(i => i.Cas)
                        .Where(i => i.Cas.PredmetId == id)
                        .ToListAsync();

                    if (izostanci.Any())
                    {
                        _context.Izostanak.RemoveRange(izostanci);
                        await _context.SaveChangesAsync();
                    }

                    // 2. Obrisi sve evidencije časova ovog predmeta
                    var evidencije = await _context.EvidencijaCasa
                        .Include(e => e.Cas)
                        .Where(e => e.Cas.PredmetId == id)
                        .ToListAsync();

                    if (evidencije.Any())
                    {
                        _context.EvidencijaCasa.RemoveRange(evidencije);
                        await _context.SaveChangesAsync();
                    }

                    // 3. Obrisi sve ocjene ovog predmeta
                    var ocjene = await _context.Ocjena
                        .Where(o => o.PredmetId == id)
                        .ToListAsync();

                    if (ocjene.Any())
                    {
                        _context.Ocjena.RemoveRange(ocjene);
                        await _context.SaveChangesAsync();
                    }

                    // 4. Obrisi sve časove ovog predmeta
                    var casovi = await _context.Cas
                        .Where(c => c.PredmetId == id)
                        .ToListAsync();

                    if (casovi.Any())
                    {
                        _context.Cas.RemoveRange(casovi);
                        await _context.SaveChangesAsync();
                    }

                    // 5. Obrisi aktivnosti povezane sa predmetom
                    var aktivnosti = await _context.Aktivnost
                        .Where(a => a.PredmetId == id)
                        .ToListAsync();

                    if (aktivnosti.Any())
                    {
                        // Prvo obrisi ObavjestenjeLog povezane sa aktivnostima
                        var aktivnostIds = aktivnosti.Select(a => a.Id).ToList();
                        var obavjestenja = await _context.ObavjestenjeLog
                            .Where(o => aktivnostIds.Contains(o.AktivnostId))
                            .ToListAsync();

                        if (obavjestenja.Any())
                        {
                            _context.ObavjestenjeLog.RemoveRange(obavjestenja);
                            await _context.SaveChangesAsync();
                        }

                        _context.Aktivnost.RemoveRange(aktivnosti);
                        await _context.SaveChangesAsync();
                    }

                    // 6. Obrisi PredmetRazred veze
                    var predmetRazred = await _context.PredmetRazred
                        .Where(pr => pr.PredmetId == id)
                        .ToListAsync();

                    if (predmetRazred.Any())
                    {
                        _context.PredmetRazred.RemoveRange(predmetRazred);
                        await _context.SaveChangesAsync();
                    }

                    // 7. Konačno obrisi predmet
                    _context.Predmet.Remove(predmet);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    TempData["Uspjeh"] = $"Predmet '{predmetNaziv}' je uspješno obrisan zajedno sa {brojOcjena} ocjena, {brojCasova} časova i {brojIzostanaka} izostanaka.";
                    return RedirectToAction("Index");
                }
                catch (Exception innerEx)
                {
                    await transaction.RollbackAsync();

                    // Detaljne informacije o grešci
                    string detaljneGreske = innerEx.Message;
                    if (innerEx.InnerException != null)
                    {
                        detaljneGreske += " | Inner: " + innerEx.InnerException.Message;
                    }

                    TempData["Greska"] = $"Greška pri brisanju predmeta: {detaljneGreske}";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                string detaljneGreske = ex.Message;
                if (ex.InnerException != null)
                {
                    detaljneGreske += " | Inner: " + ex.InnerException.Message;
                }

                TempData["Greska"] = $"Općenita greška: {detaljneGreske}";
                return RedirectToAction("Index");
            }
        }
    }
}