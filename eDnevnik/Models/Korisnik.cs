using eDnevnik.Data.@enum;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace eDnevnik.Models
{
    public class Korisnik : IdentityUser
    {


       
        public string Ime { get; set; } = "";

      
        public string Prezime { get; set; } = "";

      
        public StatusVladanja Vladanje { get; set; } = StatusVladanja.Primjereno;

        [ForeignKey("Razred")]
        public int RazredId { get; set; }
        public Razred Razred { get; set; }

        [ForeignKey("Korisnik")]
        public int RoditeljId { get; set; }
        public Korisnik Roditelj{ get; set; }

        public Korisnik() { }
    }
}
