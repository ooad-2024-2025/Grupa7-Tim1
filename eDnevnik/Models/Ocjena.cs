using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace eDnevnik.Models
{

    public class Ocjena
    {
        [Key]
        public int Id { get; set; }

       
        [ForeignKey("Korisnik")]
        public int UcenikId { get; set; }
        public Korisnik Ucenik { get; set; }

       
        [ForeignKey("Predmet")]
        public int PredmetId { get; set; }
        public Predmet Predmet { get; set; }
        public int Vrijednost { set; get; }

        public string? Komentar { get; set; }

        public DateTime Datum { get; set; }
        public Ocjena() { }
    }

}
