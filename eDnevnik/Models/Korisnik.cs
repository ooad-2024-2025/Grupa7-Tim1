
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using eDnevnik.Data.@enum;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace eDnevnik.Models
{
    public class Korisnik : IdentityUser
    {


       
        public string Ime { get; set; }

      
        public string Prezime { get; set; } 



        public StatusVladanja Vladanje { get; set; }

        [ForeignKey("Razred")]
        public int RazredId { get; set; }
        public Razred Razred { get; set; }

        [ForeignKey("Korisnik")]
       public string? RoditeljId { get; set; }
        public Korisnik Roditelj{ get; set; }

    }
}
