using eDnevnik.Data;
using eDnevnik.Models;
using eDnevnik.Data.@enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace eDnevnik.Services
{
    public class VladanjeService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;

        public VladanjeService(ApplicationDbContext context, UserManager<Korisnik> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Izračunaj vladanje na osnovu broja NEOPRAVDANIH izostanaka
        /// </summary>
        public StatusVladanja IzracunajVladanje(int brojNeopravdanihIzostanaka)
        {
            return brojNeopravdanihIzostanaka switch
            {
                <= 5 => StatusVladanja.Primjereno,
                <= 15 => StatusVladanja.VrloDobro,
                <= 25 => StatusVladanja.Dobro,
                <= 35 => StatusVladanja.Zadovoljava,
                _ => StatusVladanja.Neprimjereno
            };
        }

        /// <summary>
        /// Ažuriraj vladanje za određenog učenika
        /// </summary>
        public async Task<bool> AzurirajVladanjeUcenika(string ucenikId)
        {
            try
            {
                var ucenik = await _userManager.FindByIdAsync(ucenikId);
                if (ucenik == null) return false;

                // Provjeri da li je učenik
                var roles = await _userManager.GetRolesAsync(ucenik);
                if (!roles.Contains("Ucenik")) return false;

                // Izbroji SAMO NEOPRAVDANE izostanke za ovaj semestar/godinu
                var brojNeopravdanihIzostanaka = await _context.Izostanak
                    .Where(i => i.UcenikId == ucenikId && i.status == StatusIzostanka.Neopravdan)
                    .CountAsync();

                // Izračunaj novo vladanje na osnovu neopravdanih izostanaka
                var novoVladanje = IzracunajVladanje(brojNeopravdanihIzostanaka);

                // Ažuriraj ako se promjenilo
                if (ucenik.Vladanje != novoVladanje)
                {
                    ucenik.Vladanje = novoVladanje;
                    await _userManager.UpdateAsync(ucenik);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ažuriraj vladanje za sve učenike
        /// </summary>
        public async Task<int> AzurirajVladanjeSvihUcenika()
        {
            int azuriranoUcenika = 0;

            try
            {
                var sviKorisnici = await _userManager.Users.ToListAsync();

                foreach (var korisnik in sviKorisnici)
                {
                    var roles = await _userManager.GetRolesAsync(korisnik);
                    if (roles.Contains("Ucenik"))
                    {
                        var uspjesno = await AzurirajVladanjeUcenika(korisnik.Id);
                        if (uspjesno) azuriranoUcenika++;
                    }
                }
            }
            catch
            {
                // Log greška
            }

            return azuriranoUcenika;
        }

        /// <summary>
        /// Dobij statistike vladanja za razred
        /// </summary>
        public async Task<Dictionary<StatusVladanja, int>> StatistikeVladanjaZaRazred(int razredId)
        {
            var statistike = new Dictionary<StatusVladanja, int>();

            foreach (StatusVladanja status in Enum.GetValues<StatusVladanja>())
            {
                statistike[status] = 0;
            }

            var ucenici = await _userManager.Users
                .Where(u => u.RazredId == razredId)
                .ToListAsync();

            foreach (var ucenik in ucenici)
            {
                var roles = await _userManager.GetRolesAsync(ucenik);
                if (roles.Contains("Ucenik"))
                {
                    statistike[ucenik.Vladanje]++;
                }
            }

            return statistike;
        }

        /// <summary>
        /// Dobij detaljne informacije o vladanju učenika
        /// </summary>
        public async Task<VladanjeInfo> DetaljiVladanjaUcenika(string ucenikId)
        {
            var ucenik = await _userManager.FindByIdAsync(ucenikId);
            if (ucenik == null) return null;

            var ukupnoIzostanaka = await _context.Izostanak
                .Where(i => i.UcenikId == ucenikId)
                .CountAsync();

            var neopravdaniIzostanci = await _context.Izostanak
                .Where(i => i.UcenikId == ucenikId && i.status == StatusIzostanka.Neopravdan)
                .CountAsync();

            var opravdaniIzostanci = ukupnoIzostanaka - neopravdaniIzostanci;

            return new VladanjeInfo
            {
                TrenutnoVladanje = ucenik.Vladanje,
                UkupnoIzostanaka = ukupnoIzostanaka,
                NeopravdaniIzostanci = neopravdaniIzostanci,
                OpravdaniIzostanci = opravdaniIzostanci,
                SlijedeciNivo = GetSlijedeciNivoVladanja(neopravdaniIzostanci),
                IzostanaciDoSlijedecegNivoa = GetIzostankeDoSlijedecegNivoa(neopravdaniIzostanci)
            };
        }

        private StatusVladanja? GetSlijedeciNivoVladanja(int trenutniNeopravdaniIzostanci)
        {
            if (trenutniNeopravdaniIzostanci <= 5) return StatusVladanja.VrloDobro;
            if (trenutniNeopravdaniIzostanci <= 15) return StatusVladanja.Dobro;
            if (trenutniNeopravdaniIzostanci <= 25) return StatusVladanja.Zadovoljava;
            if (trenutniNeopravdaniIzostanci <= 35) return StatusVladanja.Neprimjereno;
            return null; // Već najgore
        }

        private int GetIzostankeDoSlijedecegNivoa(int trenutniNeopravdaniIzostanci)
        {
            if (trenutniNeopravdaniIzostanci <= 5) return 6 - trenutniNeopravdaniIzostanci;
            if (trenutniNeopravdaniIzostanci <= 15) return 16 - trenutniNeopravdaniIzostanci;
            if (trenutniNeopravdaniIzostanci <= 25) return 26 - trenutniNeopravdaniIzostanci;
            if (trenutniNeopravdaniIzostanci <= 35) return 36 - trenutniNeopravdaniIzostanci;
            return 0; // Već na najgorem nivou
        }
    }

    public class VladanjeInfo
    {
        public StatusVladanja TrenutnoVladanje { get; set; }
        public int UkupnoIzostanaka { get; set; }
        public int NeopravdaniIzostanci { get; set; }
        public int OpravdaniIzostanci { get; set; }
        public StatusVladanja? SlijedeciNivo { get; set; }
        public int IzostanaciDoSlijedecegNivoa { get; set; }
    }
}