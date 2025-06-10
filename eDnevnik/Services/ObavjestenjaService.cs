using eDnevnik.Data;
using eDnevnik.Models;
using eDnevnik.Data.@enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace eDnevnik.Services
{
    public class ObavjestenjaService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;
        private readonly EmailService _emailService;
        private readonly ILogger<ObavjestenjaService> _logger;

        public ObavjestenjaService(
            ApplicationDbContext context,
            UserManager<Korisnik> userManager,
            EmailService emailService,
            ILogger<ObavjestenjaService> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Provjeri i pošalji obavještenja za aktivnosti koje trebaju obavještenje
        /// </summary>
        public async Task<int> PošaljiObavještenjaNaRasporedu()
        {
            var danas = DateTime.Today;
            var poslanihObavještenja = 0;

            try
            {
                // Dohvati aktivnosti koje trebaju obavještenje
                var aktivnostiZaObavještenje = await _context.Aktivnost
                    .Include(a => a.Nastavnik)
                    .Include(a => a.Razred)
                    .Include(a => a.Predmet)
                    .Where(a => a.Aktivna && a.Datum > danas)
                    .ToListAsync();

                foreach (var aktivnost in aktivnostiZaObavještenje)
                {
                    var trebaPoslati = TrebaPoslatiObavještenje(aktivnost, danas);
                    if (trebaPoslati)
                    {
                        var rezultat = await PošaljiObavještenjeZaAktivnost(aktivnost);
                        if (rezultat > 0) poslanihObavještenja += rezultat;
                    }
                }

                _logger.LogInformation("Poslano {BrojObavještenja} obavještenja", poslanihObavještenja);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom slanja obavještenja");
            }

            return poslanihObavještenja;
        }

        /// <summary>
        /// Provjeri da li aktivnost treba obavještenje danas
        /// </summary>
        private bool TrebaPoslatiObavještenje(Aktivnost aktivnost, DateTime danas)
        {
            var danaDoAktivnosti = (aktivnost.Datum.Date - danas).Days;

            return aktivnost.Prioritet switch
            {
                PrioritetAktivnosti.Visok => !VećPostojiObavještenje(aktivnost.Id), // Odmah, ali samo jednom
                PrioritetAktivnosti.Srednji => danaDoAktivnosti == 3 && !VećPostojiObavještenje(aktivnost.Id), // 3 dana prije
                PrioritetAktivnosti.Nizak => danaDoAktivnosti == 1 && !VećPostojiObavještenje(aktivnost.Id), // 1 dan prije
                _ => false
            };
        }

        /// <summary>
        /// Provjeri da li je već poslano obavještenje za aktivnost
        /// </summary>
        private bool VećPostojiObavještenje(int aktivnostId)
        {
            return _context.ObavjestenjeLog
                .Any(o => o.AktivnostId == aktivnostId && o.Status == StatusObavjestenja.Poslano);
        }

        /// <summary>
        /// Pošalji obavještenje za određenu aktivnost
        /// </summary>
        public async Task<int> PošaljiObavještenjeZaAktivnost(Aktivnost aktivnost)
        {
            var poslanihObavještenja = 0;

            try
            {
                // Dohvati korisnike kojima treba poslati obavještenje
                var korisnici = await DohvatiKorisnikeZaObavještenje(aktivnost);

                foreach (var korisnik in korisnici)
                {
                    if (!string.IsNullOrEmpty(korisnik.Email))
                    {
                        var sadržaj = KreirajSadržajObavještenja(aktivnost, korisnik);
                        var naslov = $"eDnevnik - {aktivnost.TipText}: {aktivnost.Naziv}";

                        var uspješno = await _emailService.PošaljiEmailAsync(korisnik.Email, naslov, sadržaj);

                        // Logiraj rezultat
                        var log = new ObavjestenjeLog
                        {
                            AktivnostId = aktivnost.Id,
                            KorisnikId = korisnik.Id,
                            EmailAdresa = korisnik.Email,
                            Status = uspješno ? StatusObavjestenja.Poslano : StatusObavjestenja.Greška,
                            SadržajEmaila = sadržaj,
                            BrojPokušaja = 1,
                            Greska = uspješno ? null : "Greška prilikom slanja email-a"
                        };

                        _context.ObavjestenjeLog.Add(log);

                        if (uspješno) poslanihObavještenja++;
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom slanja obavještenja za aktivnost {AktivnostId}", aktivnost.Id);
            }

            return poslanihObavještenja;
        }

        /// <summary>
        /// Dohvati korisnike kojima treba poslati obavještenje
        /// </summary>
        private async Task<List<Korisnik>> DohvatiKorisnikeZaObavještenje(Aktivnost aktivnost)
        {
            var korisnici = new List<Korisnik>();

            // Učenici
            if (aktivnost.RazredId.HasValue)
            {
                // Specifičan razred
                var uceniciRazreda = await _userManager.Users
                    .Where(u => u.RazredId == aktivnost.RazredId)
                    .ToListAsync();

                foreach (var ucenik in uceniciRazreda)
                {
                    var roles = await _userManager.GetRolesAsync(ucenik);
                    if (roles.Contains("Ucenik"))
                    {
                        korisnici.Add(ucenik);

                        // Dodaj i roditelja ako postoji
                        if (!string.IsNullOrEmpty(ucenik.RoditeljId))
                        {
                            var roditelj = await _userManager.FindByIdAsync(ucenik.RoditeljId);
                            if (roditelj != null && !korisnici.Contains(roditelj))
                            {
                                korisnici.Add(roditelj);
                            }
                        }
                    }
                }
            }
            else
            {
                // Svi učenici
                var sviKorisnici = await _userManager.Users.ToListAsync();
                foreach (var korisnik in sviKorisnici)
                {
                    var roles = await _userManager.GetRolesAsync(korisnik);
                    if (roles.Contains("Ucenik"))
                    {
                        korisnici.Add(korisnik);

                        // Dodaj i roditelja
                        if (!string.IsNullOrEmpty(korisnik.RoditeljId))
                        {
                            var roditelj = await _userManager.FindByIdAsync(korisnik.RoditeljId);
                            if (roditelj != null && !korisnici.Contains(roditelj))
                            {
                                korisnici.Add(roditelj);
                            }
                        }
                    }
                }
            }

            return korisnici.Where(k => !string.IsNullOrEmpty(k.Email)).ToList();
        }

        /// <summary>
        /// Kreiraj sadržaj email obavještenja
        /// </summary>
        private string KreirajSadržajObavještenja(Aktivnost aktivnost, Korisnik korisnik)
        {
            var roles = _userManager.GetRolesAsync(korisnik).Result;
            var jeRoditelj = roles.Contains("Roditelj");

            var naslov = $"{aktivnost.TipText}: {aktivnost.Naziv}";
            var datum = aktivnost.Datum.ToString("dddd, dd.MM.yyyy u HH:mm");

            var poruka = jeRoditelj
                ? $"Poštovani roditelju, obavještavamo Vas o nadolazećoj aktivnosti:"
                : $"Poštovani {korisnik.Ime}, obavještavamo Vas o nadolazećoj aktivnosti:";

            var dodatneInfo = $@"
                <div style='background-color: #e9ecef; padding: 15px; margin: 15px 0; border-radius: 5px;'>
                    <p><strong>Tip:</strong> {aktivnost.TipText}</p>
                    <p><strong>Datum:</strong> {datum}</p>
                    <p><strong>Opis:</strong> {aktivnost.Opis}</p>
                    {(aktivnost.Predmet != null ? $"<p><strong>Predmet:</strong> {aktivnost.Predmet.Naziv}</p>" : "")}
                    <p><strong>Ciljana grupa:</strong> {aktivnost.CiljanaGrupa}</p>
                    <p><strong>Prioritet:</strong> <span style='color: {GetPrioritetColor(aktivnost.Prioritet)}'>{aktivnost.PrioritetText}</span></p>
                </div>";

            return _emailService.KreirajHtmlSadržaj(naslov, poruka, dodatneInfo);
        }

        private string GetPrioritetColor(PrioritetAktivnosti prioritet)
        {
            return prioritet switch
            {
                PrioritetAktivnosti.Visok => "#dc3545",
                PrioritetAktivnosti.Srednji => "#ffc107",
                PrioritetAktivnosti.Nizak => "#17a2b8",
                _ => "#6c757d"
            };
        }

        /// <summary>
        /// Ponovi neuspješna obavještenja
        /// </summary>
        public async Task<int> PonociNeuspješnaObavještenja()
        {
            var neuspješnaObavještenja = await _context.ObavjestenjeLog
                .Include(o => o.Aktivnost)
                .Include(o => o.Korisnik)
                .Where(o => o.TrebaPonovo)
                .ToListAsync();

            var ponovljeno = 0;

            foreach (var log in neuspješnaObavještenja)
            {
                if (log.Aktivnost != null && log.Korisnik != null)
                {
                    var sadržaj = log.SadržajEmaila ?? KreirajSadržajObavještenja(log.Aktivnost, log.Korisnik);
                    var naslov = $"eDnevnik - {log.Aktivnost.TipText}: {log.Aktivnost.Naziv}";

                    var uspješno = await _emailService.PošaljiEmailAsync(log.EmailAdresa, naslov, sadržaj);

                    log.BrojPokušaja++;
                    log.VrijemeSlanja = DateTime.Now;

                    if (uspješno)
                    {
                        log.Status = StatusObavjestenja.Poslano;
                        log.Greska = null;
                        ponovljeno++;
                    }
                    else
                    {
                        log.VrijemeSlijedecegPokušaja = DateTime.Now.AddHours(1); // Pokušaj ponovo za 1 sat
                        log.Greska = "Ponovna greška prilikom slanja";
                    }
                }
            }

            await _context.SaveChangesAsync();
            return ponovljeno;
        }
    }
}