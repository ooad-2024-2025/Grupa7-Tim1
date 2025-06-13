using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eDnevnik.Models
{
    public class Predmet
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv predmeta je obavezan")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Naziv mora imati između 2 i 100 karaktera")]
        [Display(Name = "Naziv predmeta")]
        public string Naziv { get; set; }

        [StringLength(500, ErrorMessage = "Opis može imati maksimalno 500 karaktera")]
        [Display(Name = "Opis predmeta")]
        public string? Opis { get; set; }

        [Display(Name = "Nastavnik")]
        [ForeignKey("Korisnik")]
        public string? NastavnikId { set; get; }
        public Korisnik? Nastavnik { get; set; }

        public Predmet() { }
    }
}