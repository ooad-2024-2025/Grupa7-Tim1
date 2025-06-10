using eDnevnik.Models;
using eDnevnik.Services;
using eDnevnik.Data.@enum;

namespace eDnevnik.ViewModels
{
    public class UcenikDashboardViewModel
    {
        public Korisnik Ucenik { get; set; } = new();
        public List<OcjenaPoPredmetu> OcjenePoPremetima { get; set; } = new();
        public List<Izostanak> RecentniIzostanci { get; set; } = new();
        public VladanjeInfo VladanjeInfo { get; set; } = new();
        public UcenikStatistike Statistike { get; set; } = new();
        public List<ActivityItem> RecentneAktivnosti { get; set; } = new();
        public List<Cas> DanasnjiCasovi { get; set; } = new();
    }

    public class OcjenaPoPredmetu
    {
        public Predmet Predmet { get; set; } = new();
        public List<Ocjena> Ocjene { get; set; } = new();
        public double Prosjek { get; set; }
        public int BrojOcjena { get; set; }
        public Ocjena? NajnovijaOcjena { get; set; }
        public string ProsjekText => Prosjek > 0 ? Prosjek.ToString("F2") : "Nema ocjena";
        public string ProsjekClass => Prosjek >= 4.5 ? "text-success" :
                                     Prosjek >= 3.5 ? "text-warning" :
                                     Prosjek >= 2.0 ? "text-danger" : "text-muted";
    }

    public class UcenikStatistike
    {
        public double OpciProsjek { get; set; }
        public int UkupnoOcjena { get; set; }
        public int UkupnoIzostanaka { get; set; }
        public int NeopravdaniIzostanci { get; set; }
        public int OpravdaniIzostanci { get; set; }
        public int BrojPredmeta { get; set; }
        public Dictionary<int, int> OcjeneDistribucija { get; set; } = new();

        public string OpciProsjekText => OpciProsjek > 0 ? OpciProsjek.ToString("F2") : "Nema ocjena";
        public string OpciProsjekClass => OpciProsjek >= 4.5 ? "text-success" :
                                         OpciProsjek >= 3.5 ? "text-warning" :
                                         OpciProsjek >= 2.0 ? "text-danger" : "text-muted";
    }

    public class ActivityItem
    {
        public string Tip { get; set; } = string.Empty; // "Ocjena", "Izostanak", "Evidencija"
        public string Opis { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public string Ikona { get; set; } = string.Empty;
        public string CssClass { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;

        public string RelativnoVrijeme
        {
            get
            {
                var razlika = DateTime.Now - Datum;
                if (razlika.TotalMinutes < 60) return $"Prije {(int)razlika.TotalMinutes} min";
                if (razlika.TotalHours < 24) return $"Prije {(int)razlika.TotalHours} h";
                if (razlika.TotalDays < 7) return $"Prije {(int)razlika.TotalDays} dana";
                return Datum.ToString("dd.MM.yyyy");
            }
        }
    }

    public class UcenikOcjeneViewModel
    {
        public Korisnik Ucenik { get; set; } = new();
        public List<OcjenaPoPredmetu> OcjenePoPremetima { get; set; } = new();
        public UcenikStatistike Statistike { get; set; } = new();
        public string SelectedPredmet { get; set; } = "svi";
        public DateTime? OdDatuma { get; set; }
        public DateTime? DoDatuma { get; set; }
    }

    public class UcenikIzostanciViewModel
    {
        public Korisnik Ucenik { get; set; } = new();
        public List<IzostanakSaDetaljima> Izostanci { get; set; } = new();
        public VladanjeInfo VladanjeInfo { get; set; } = new();
        public Dictionary<string, int> IzostanciPoMjesecima { get; set; } = new();
        public string SelectedStatus { get; set; } = "svi";
        public DateTime? OdDatuma { get; set; }
        public DateTime? DoDatuma { get; set; }
    }

    public class IzostanakSaDetaljima
    {
        public Izostanak Izostanak { get; set; } = new();
        public string PredmetNaziv { get; set; } = string.Empty;
        public string TerminInfo { get; set; } = string.Empty;
        public DateTime DatumCasa { get; set; }
    }
}