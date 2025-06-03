using eDnevnik.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eDnevnik.Models
{
    public class Razred
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv je obavezan.")]
        public string Naziv { get; set; } // Sada sadrži i godinu i odjeljenje, npr. "IV-b"

        [Required(ErrorMessage = "Razrednik je obavezan.")]
        public string NastavnikId { get; set; }

        public Korisnik? Nastavnik { get; set; }
    }

}

