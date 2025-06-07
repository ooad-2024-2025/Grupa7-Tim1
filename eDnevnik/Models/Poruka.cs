using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eDnevnik.Models
{
    public class Poruka
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Korisnik")]
        public string PosiljalacId { get; set; }
        public Korisnik Posiljalac { get; set; }

        [ForeignKey("Korisnik")]
        public string? PrimalacId { get; set; }  // Dozvoljen NULL
        public Korisnik? Primalac { get; set; }



        [Required]
        public string Sadrzaj { get; set; }

        public DateTime VrijemeSlanja { get; set; } = DateTime.Now;
    }
}
