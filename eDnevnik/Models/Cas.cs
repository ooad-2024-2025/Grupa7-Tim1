using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eDnevnik.Models
{
    public class Cas
    {
        [Key]
        public int Id { set; get; }

        // POSTOJEĆI - ZADRŽI
        public DateTime Termin { set; get; }

        // NOVI - DODANO za fiksne termine
        public DayOfWeek? DanUSedmici { get; set; }
        public int? FixniTerminId { get; set; }

        [ForeignKey("FixniTerminId")]
        public FixniTermin? FixniTermin { get; set; }

        public int RazredId { set; get; }
        public Razred? Razred { set; get; }

        [ForeignKey("Predmet")]
        public int PredmetId { set; get; }
        public Predmet? Predmet { set; get; }

        [ForeignKey("Korisnik")]
        public string NastavnikId { set; get; }
        public Korisnik? Nastavnik { set; get; }

        public Cas() { }

        // Computed properties za novi raspored
        [NotMapped]
        public string DanNaziv
        {
            get
            {
                if (DanUSedmici == null) return "Nepoznat dan";
                return DanUSedmici switch
                {
                    DayOfWeek.Monday => "Ponedjeljak",
                    DayOfWeek.Tuesday => "Utorak",
                    DayOfWeek.Wednesday => "Srijeda",
                    DayOfWeek.Thursday => "Četvrtak",
                    DayOfWeek.Friday => "Petak",
                    _ => "Nepoznat dan"
                };
            }
        }
    }
}