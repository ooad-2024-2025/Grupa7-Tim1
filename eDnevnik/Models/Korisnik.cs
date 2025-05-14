using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace eDnevnik.Models
{
    public class Korisnik : IdentityUser
    {
        public string Ime { get;set;}

        public string Prezime { get; set; }


    }
}
