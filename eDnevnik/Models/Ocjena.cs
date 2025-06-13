using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eDnevnik.Models
{
    public class Ocjena
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Učenik je obavezan")]
        [ForeignKey("Korisnik")]
        public string UcenikId { get; set; }
        public Korisnik? Ucenik { get; set; }

        [Required(ErrorMessage = "Predmet je obavezan")]
        [ForeignKey("Predmet")]
        public int PredmetId { get; set; }
        public Predmet? Predmet { get; set; }

        [Required(ErrorMessage = "Vrijednost ocjene je obavezna")]
        [Range(1, 5, ErrorMessage = "Ocjena mora biti između 1 i 5")]
        [Display(Name = "Ocjena")]
        public int Vrijednost { set; get; }

        [StringLength(200, ErrorMessage = "Komentar može imati maksimalno 200 karaktera")]
        [Display(Name = "Komentar")]
        public string? Komentar { get; set; }

        public DateTime Datum { get; set; } = DateTime.Now;

        public Ocjena() { }

        // Computed property
        [NotMapped]
        public string OcjenaText => Vrijednost switch
        {
            1 => "Nedovoljan (1)",
            2 => "Dovoljan (2)",
            3 => "Dobar (3)",
            4 => "Vrlo dobar (4)",
            5 => "Odličan (5)",
            _ => "Nepoznato"
        };
    }
}