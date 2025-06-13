using eDnevnik.Models;

namespace eDnevnik.ViewModels
{
    public class RoditeljDashboardViewModel
    {
        public Korisnik Roditelj { get; set; } = new();
        public List<DijeteInfo> Djeca { get; set; } = new();
        public RoditeljStatistike Statistike { get; set; } = new();
        public List<RecentnaAktivnost> RecentneAktivnosti { get; set; } = new();
    }

    public class DijeteInfo
    {
        public Korisnik Ucenik { get; set; } = new();
        public string RazredNaziv { get; set; } = string.Empty;
        public double OpciProsjek { get; set; }
        public int BrojOcjena { get; set; }
        public int BrojIzostanaka { get; set; }
        public string VladanjeText { get; set; } = string.Empty;
        public string VladanjeClass { get; set; } = string.Empty;
        public Ocjena? NajnovijaOcjena { get; set; }
        public List<Ocjena> PosledneOcjene { get; set; } = new();

        // Computed properties
        public string OpciProsjekText => OpciProsjek > 0 ? OpciProsjek.ToString("F2") : "Nema ocjena";
        public string OpciProsjekClass => OpciProsjek switch
        {
            >= 4.5 => "text-success",
            >= 3.5 => "text-warning",
            >= 2.0 => "text-danger",
            _ => "text-muted"
        };
    }

    public class RoditeljStatistike
    {
        public int UkupnoDjece { get; set; }
        public double ProsjekSveDjece { get; set; }
        public int UkupnoOcjena { get; set; }
        public int UkupnoIzostanaka { get; set; }
        public DijeteInfo? NajboljeDijete { get; set; }
        public int BrojPredmeta { get; set; }

        // Computed properties
        public string ProsjekSveDjeceText => ProsjekSveDjece > 0 ? ProsjekSveDjece.ToString("F2") : "N/A";
        public string ProsjekSveDjeceClass => ProsjekSveDjece switch
        {
            >= 4.5 => "text-success",
            >= 3.5 => "text-warning",
            >= 2.0 => "text-danger",
            _ => "text-muted"
        };
    }

    public class RecentnaAktivnost
    {
        public string Tip { get; set; } = string.Empty; // "ocjena", "izostanak", "aktivnost"
        public string Opis { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public string DijeteIme { get; set; } = string.Empty;
        public string IkonaKlasa { get; set; } = string.Empty;
        public string BojaKlase { get; set; } = string.Empty;

        // Computed properties
        public string RelativnoVrijeme
        {
            get
            {
                // Provjeri da li je datum valjan
                if (Datum == DateTime.MinValue || Datum == default(DateTime))
                {
                    return "Nepoznato vrijeme";
                }

                var razlika = DateTime.Now - Datum;

                if (razlika.TotalMinutes < 60)
                    return $"Prije {(int)razlika.TotalMinutes} min";
                if (razlika.TotalHours < 24)
                    return $"Prije {(int)razlika.TotalHours} h";
                if (razlika.TotalDays < 7)
                    return $"Prije {(int)razlika.TotalDays} dana";
                if (razlika.TotalDays < 30)
                    return $"Prije {(int)(razlika.TotalDays / 7)} sedmica";

                return Datum.ToString("dd.MM.yyyy");
            }
        }
    }
}