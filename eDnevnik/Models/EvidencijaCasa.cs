using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eDnevnik.Models
{
    public class EvidencijaCasa
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Cas")]
        public int CasId { get; set; }
        public Cas? Cas { get; set; }

        [Required]
        public DateTime DatumOdrzavanja { get; set; } = DateTime.Now;

        public string? Aktivnosti { get; set; } // Nastavne jedinice/aktivnosti

        public string? Napomene { get; set; } // Dodatne napomene

        public bool Odrzan { get; set; } = true; // Da li je čas održan

        [ForeignKey("Korisnik")]
        public string NastavnikId { get; set; } = string.Empty;
        public Korisnik? Nastavnik { get; set; }

        public DateTime VrijemeEvidentiranja { get; set; } = DateTime.Now;

        // Computed properties
        [NotMapped]
        public string StatusTekst => Odrzan ? "Održan" : "Otkazan";

        [NotMapped]
        public string KratkiOpis => !string.IsNullOrEmpty(Aktivnosti) && Aktivnosti.Length > 50
            ? Aktivnosti.Substring(0, 50) + "..."
            : Aktivnosti ?? "Bez opisa";

        public EvidencijaCasa() { }
    }
}