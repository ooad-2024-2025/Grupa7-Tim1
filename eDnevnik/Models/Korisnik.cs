using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using eDnevnik.Data.@enum;
using System.ComponentModel.DataAnnotations;

namespace eDnevnik.Models
{
    public class Korisnik : IdentityUser
    {
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string? Adresa { get; set; }
        public string? Telefon { get; set; }

        public StatusVladanja Vladanje { get; set; }

        [ForeignKey("Razred")]
        public int? RazredId { get; set; }
        public Razred? Razred { get; set; }

        [ForeignKey("Korisnik")]
        public string? RoditeljId { get; set; }
        public Korisnik? Roditelj { get; set; }

        [NotMapped]
        public string FullName => $"{Ime} {Prezime}";

    }
}
