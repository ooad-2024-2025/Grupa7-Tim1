using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using eDnevnik.Data.@enum;

namespace eDnevnik.Models
{
    public class ObavjestenjeLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Aktivnost")]
        public int AktivnostId { get; set; }
        public Aktivnost? Aktivnost { get; set; }

        [Required]
        [ForeignKey("Korisnik")]
        public string KorisnikId { get; set; } = string.Empty;
        public Korisnik? Korisnik { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAdresa { get; set; } = string.Empty;

        [Required]
        public DateTime VrijemeSlanja { get; set; } = DateTime.Now;

        [Required]
        public StatusObavjestenja Status { get; set; }

        public string? Greska { get; set; }

        public string? SadržajEmaila { get; set; }

        public int BrojPokušaja { get; set; } = 0;

        public DateTime? VrijemeSlijedecegPokušaja { get; set; }

        // Computed properties
        [NotMapped]
        public string StatusText => Status switch
        {
            StatusObavjestenja.Čeka => "Čeka slanje",
            StatusObavjestenja.Poslano => "Poslano",
            StatusObavjestenja.Greška => "Greška",
            StatusObavjestenja.Preskočeno => "Preskočeno",
            _ => "Nepoznato"
        };

        [NotMapped]
        public string StatusClass => Status switch
        {
            StatusObavjestenja.Čeka => "warning",
            StatusObavjestenja.Poslano => "success",
            StatusObavjestenja.Greška => "danger",
            StatusObavjestenja.Preskočeno => "secondary",
            _ => "light"
        };

        [NotMapped]
        public bool TrebaPonovo => Status == StatusObavjestenja.Greška &&
                                  BrojPokušaja < 3 &&
                                  VrijemeSlijedecegPokušaja <= DateTime.Now;
    }
}