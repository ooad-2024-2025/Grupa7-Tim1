using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using eDnevnik.Data.@enum;

namespace eDnevnik.Models
{
    public class Aktivnost
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv je obavezan")]
        [StringLength(200, ErrorMessage = "Naziv može imati maksimalno 200 karaktera")]
        public string Naziv { get; set; } = string.Empty;

        [Required(ErrorMessage = "Opis je obavezan")]
        [StringLength(1000, ErrorMessage = "Opis može imati maksimalno 1000 karaktera")]
        public string Opis { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum je obavezan")]
        public DateTime Datum { get; set; }

        [Required(ErrorMessage = "Tip aktivnosti je obavezan")]
        public TipAktivnosti Tip { get; set; }

        [Required(ErrorMessage = "Prioritet je obavezan")]
        public PrioritetAktivnosti Prioritet { get; set; }

        // PROMJENA: Ukloni [Required] jer ćemo postavljati programski
        [ForeignKey("Korisnik")]
        public string NastavnikId { get; set; } = string.Empty;
        public Korisnik? Nastavnik { get; set; }

        [ForeignKey("Razred")]
        public int? RazredId { get; set; } // Null = svi razredi
        public Razred? Razred { get; set; }

        [ForeignKey("Predmet")]
        public int? PredmetId { get; set; } // Null = vannastavna aktivnost
        public Predmet? Predmet { get; set; }

        public DateTime DatumKreiranja { get; set; } = DateTime.Now;

        public bool Aktivna { get; set; } = true;

        // Computed properties
        [NotMapped]
        public string TipText => Tip switch
        {
            TipAktivnosti.Test => "Test",
            TipAktivnosti.Zadaca => "Zadaća",
            TipAktivnosti.Takmicenje => "Takmičenje",
            TipAktivnosti.SkolarskiDogadjaj => "Školarski događaj",
            TipAktivnosti.Prezentacija => "Prezentacija",
            TipAktivnosti.Ekskurzija => "Ekskurzija",
            _ => "Ostalo"
        };

        [NotMapped]
        public string PrioritetText => Prioritet switch
        {
            PrioritetAktivnosti.Visok => "Visok",
            PrioritetAktivnosti.Srednji => "Srednji",
            PrioritetAktivnosti.Nizak => "Nizak",
            _ => "Nepoznat"
        };

        [NotMapped]
        public string TipClass => Tip switch
        {
            TipAktivnosti.Test => "danger",
            TipAktivnosti.Zadaca => "warning",
            TipAktivnosti.Takmicenje => "success",
            TipAktivnosti.SkolarskiDogadjaj => "primary",
            TipAktivnosti.Prezentacija => "info",
            TipAktivnosti.Ekskurzija => "secondary",
            _ => "light"
        };

        [NotMapped]
        public string PrioritetClass => Prioritet switch
        {
            PrioritetAktivnosti.Visok => "danger",
            PrioritetAktivnosti.Srednji => "warning",
            PrioritetAktivnosti.Nizak => "info",
            _ => "secondary"
        };

        [NotMapped]
        public bool JeProšla => Datum < DateTime.Now;

        [NotMapped]
        public bool JeDanas => Datum.Date == DateTime.Today;

        [NotMapped]
        public bool JeSutra => Datum.Date == DateTime.Today.AddDays(1);

        [NotMapped]
        public int DanaDoAktivnosti => (int)(Datum.Date - DateTime.Today).TotalDays;

        [NotMapped]
        public string StatusText
        {
            get
            {
                if (JeProšla) return "Završena";
                if (JeDanas) return "Danas";
                if (JeSutra) return "Sutra";
                if (DanaDoAktivnosti <= 7) return $"Za {DanaDoAktivnosti} dana";
                return Datum.ToString("dd.MM.yyyy");
            }
        }

        [NotMapped]
        public string CiljanaGrupa
        {
            get
            {
                if (RazredId.HasValue && Razred != null)
                    return $"Razred {Razred.Naziv}";
                return "Svi razredi";
            }
        }
    }
}