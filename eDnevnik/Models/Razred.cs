using eDnevnik.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eDnevnik.Models
{
    public class Razred
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv razreda je obavezan")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Naziv mora imati između 2 i 10 karaktera")]
        [RegularExpression(@"^[IVX]+-[a-zA-Z]$", ErrorMessage = "Format: Rimski broj + crtica + slovo (npr. IV-b)")]
        [Display(Name = "Naziv razreda")]
        public string Naziv { get; set; } // Sada sadrži i godinu i odjeljenje, npr. "IV-b"

        [Required(ErrorMessage = "Razrednik je obavezan")]
        [Display(Name = "Razrednik")]
        public string NastavnikId { get; set; }

        public Korisnik? Nastavnik { get; set; }
    }
}