using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eDnevnik.Models
{
    public class Cas
    {
        [Key]
        public int Id { set; get; }

        public DateTime Termin { set; get; }

        public int RazredId { set; get; }
        public Razred Razred { set; get; }

        [ForeignKey("Predmet")]
        public int PredmetId { set; get; }
        public Predmet Predmet { set; get; }

        [ForeignKey("Korisnik")]
        public string NastavnikId { set; get; }
        public Korisnik Nastavnik { set; get; }
       
        public Cas() { }
    }
}
