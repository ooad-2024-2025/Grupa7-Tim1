using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace eDnevnik.Models
{


    public class UcenikPredmet
    {
        [Key]
        public int Id { get; set; }



        [ForeignKey("Korisnik")]
        public int UcenikId { get; set; }
        public Korisnik Ucenik { get; set; }



        [ForeignKey("Predmet")]
        public int PredmetId { get; set; }
        public Predmet Predmet { get; set; }
    
    public UcenikPredmet() { }

    } }
