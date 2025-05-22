using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eDnevnik.Models
{
    

    public class Predmet
    {
        [Key]
        public int Id { get; set; }

        public string Naziv { get; set; }
        public string? Opis { get; set; }

        [ForeignKey("Korisnik")]
        public string NastavnikId { set; get; }
        public Korisnik Nastavnik { get; set; }

        
        public ICollection<Cas> Casovi { get; set; }
        public ICollection<Ocjena> Ocjene { get; set; }
        public ICollection<UcenikPredmet> UcenikPredmeti { get; set; }

        public Predmet() { }
    }

}
