using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using eDnevnik.Data.@enum;
using System.ComponentModel.DataAnnotations;

namespace eDnevnik.Models
{
    public class Korisnik : IdentityUser
    {
        [Required(ErrorMessage = "Ime je obavezno")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ime mora imati između 2 i 50 karaktera")]
        [RegularExpression(@"^[A-Za-zÀ-žÀ-ÿĀ-žŠšĐđČčĆćŽž\s]+$", ErrorMessage = "Ime može sadržavati samo slova i razmake")]
        public string Ime { get; set; }

        [Required(ErrorMessage = "Prezime je obavezno")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Prezime mora imati između 2 i 50 karaktera")]
        [RegularExpression(@"^[A-Za-zÀ-žÀ-ÿĀ-žŠšĐđČčĆćŽž\s]+$", ErrorMessage = "Prezime može sadržavati samo slova i razmake")]
        public string Prezime { get; set; }

        [StringLength(50, ErrorMessage = "Adresa ne može biti duža od 50 karaktera")]
        public string? Adresa { get; set; }

        [Phone(ErrorMessage = "Neispravan format broja telefona")]
        [RegularExpression(@"^(\+387|0)[\s\-]?[0-9]{2}[\s\-]?[0-9]{3}[\s\-]?[0-9]{3,4}$|^[0-9]{3}\/[0-9]{3}\-[0-9]{3}$",
            ErrorMessage = "Unesite valjan bosanski broj telefona")]
        public string? Telefon { get; set; }

        [Required(ErrorMessage = "Email adresa je obavezna")]
        [EmailAddress(ErrorMessage = "Unesite ispravnu email adresu")]
        [StringLength(100, ErrorMessage = "Email ne može biti duži od 100 karaktera")]
        public override string Email { get; set; }

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
