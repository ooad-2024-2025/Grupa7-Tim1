using System.ComponentModel.DataAnnotations;

namespace eDnevnik.Models
{
    public class FixniTermin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Naziv { get; set; } = string.Empty;

        [Required]
        public TimeSpan PocetakVremena { get; set; }

        [Required]
        public TimeSpan KrajVremena { get; set; }

        public int Redoslijed { get; set; }

        public bool JeOdmor { get; set; } = false;

        // Computed property
        public string FormatiraniTermin => $"{PocetakVremena:hh\\:mm} - {KrajVremena:hh\\:mm}";

        // Predefined termini
        public static List<FixniTermin> GetStandardniTermini()
        {
            return new List<FixniTermin>
            {
                new FixniTermin { Id = 1, Naziv = "1. čas", PocetakVremena = new TimeSpan(8, 0, 0), KrajVremena = new TimeSpan(8, 45, 0), Redoslijed = 1 },
                new FixniTermin { Id = 2, Naziv = "2. čas", PocetakVremena = new TimeSpan(8, 50, 0), KrajVremena = new TimeSpan(9, 35, 0), Redoslijed = 2 },
                new FixniTermin { Id = 3, Naziv = "3. čas", PocetakVremena = new TimeSpan(9, 40, 0), KrajVremena = new TimeSpan(10, 25, 0), Redoslijed = 3 },

                new FixniTermin { Id = 100, Naziv = "Veliki odmor", PocetakVremena = new TimeSpan(10, 25, 0), KrajVremena = new TimeSpan(10, 45, 0), Redoslijed = 4, JeOdmor = true },

                new FixniTermin { Id = 4, Naziv = "4. čas", PocetakVremena = new TimeSpan(10, 45, 0), KrajVremena = new TimeSpan(11, 30, 0), Redoslijed = 5 },
                new FixniTermin { Id = 5, Naziv = "5. čas", PocetakVremena = new TimeSpan(11, 35, 0), KrajVremena = new TimeSpan(12, 20, 0), Redoslijed = 6 },
                new FixniTermin { Id = 6, Naziv = "6. čas", PocetakVremena = new TimeSpan(12, 25, 0), KrajVremena = new TimeSpan(13, 10, 0), Redoslijed = 7 },

                new FixniTermin { Id = 101, Naziv = "Veliki odmor", PocetakVremena = new TimeSpan(13, 10, 0), KrajVremena = new TimeSpan(13, 30, 0), Redoslijed = 8, JeOdmor = true },

                new FixniTermin { Id = 7, Naziv = "7. čas", PocetakVremena = new TimeSpan(13, 30, 0), KrajVremena = new TimeSpan(14, 15, 0), Redoslijed = 9 },
                new FixniTermin { Id = 8, Naziv = "8. čas", PocetakVremena = new TimeSpan(14, 20, 0), KrajVremena = new TimeSpan(15, 5, 0), Redoslijed = 10 },
                new FixniTermin { Id = 9, Naziv = "9. čas", PocetakVremena = new TimeSpan(15, 10, 0), KrajVremena = new TimeSpan(15, 55, 0), Redoslijed = 11 }
            };
        }
    }
}