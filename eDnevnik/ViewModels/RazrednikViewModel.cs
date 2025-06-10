using eDnevnik.Models;
using eDnevnik.Services;
using eDnevnik.Data.@enum;

namespace eDnevnik.ViewModels
{
    public class RazrednikDashboardViewModel
    {
        public Razred Razred { get; set; } = new();
        public Korisnik Nastavnik { get; set; } = new();
        public List<UcenikSaStatistikama> Ucenici { get; set; } = new();
        public RazredStatistike Statistike { get; set; } = new();
        public Dictionary<StatusVladanja, int> VladanjeStatistike { get; set; } = new();
        public List<IzostanakSaDetaljima> NeodobreniIzostanci { get; set; } = new();
    }

    public class RazrednikUceniciViewModel
    {
        public Razred Razred { get; set; } = new();
        public List<UcenikSaDetaljnimiStatistikama> UceniciSaStatistikama { get; set; } = new();
    }

    public class RazrednikIzostanciViewModel
    {
        public Razred Razred { get; set; } = new();
        public List<IzostanakSaDetaljima> Izostanci { get; set; } = new();
        public string SelectedStatus { get; set; } = "neodobreni";
    }

    public class RazrednikStatistikeViewModel
    {
        public Razred Razred { get; set; } = new();
        public RazredDetaljeStatistike Statistike { get; set; } = new();
        public Dictionary<StatusVladanja, int> VladanjeStatistike { get; set; } = new();
        public List<OcjenePoPremetima> OcjenePoPremetima { get; set; } = new();
    }

    public class UcenikSaStatistikama
    {
        public Korisnik Ucenik { get; set; } = new();
        public double OpciProsjek { get; set; }
        public int UkupnoOcjena { get; set; }
        public int UkupnoIzostanaka { get; set; }
        public int NeopravdaniIzostanci { get; set; }

        public string OpciProsjekText => OpciProsjek > 0 ? OpciProsjek.ToString("F2") : "Nema ocjena";
        public string OpciProsjekClass => OpciProsjek >= 4.5 ? "text-success" :
                                         OpciProsjek >= 3.5 ? "text-warning" :
                                         OpciProsjek >= 2.0 ? "text-danger" : "text-muted";
    }

    public class UcenikSaDetaljnimiStatistikama : UcenikSaStatistikama
    {
        public VladanjeInfo VladanjeInfo { get; set; } = new();

        public string VladanjeText => VladanjeInfo.TrenutnoVladanje.ToString() switch
        {
            "Primjerno" => "Primjerno",
            "VrloDobar" => "Vrlo dobro",
            "Dobro" => "Dobro",
            "Zadovoljava" => "Zadovoljava",
            "NeZadovoljava" => "Ne zadovoljava",
            _ => VladanjeInfo.TrenutnoVladanje.ToString()
        };

        public string VladanjeClass => VladanjeInfo.TrenutnoVladanje.ToString() switch
        {
            "Primjerno" => "text-success",
            "VrloDobar" => "text-info",
            "Dobro" => "text-primary",
            "Zadovoljava" => "text-warning",
            "NeZadovoljava" => "text-danger",
            _ => "text-muted"
        };
    }

    public class RazredStatistike
    {
        public int BrojUcenika { get; set; }
        public double ProsjekRazreda { get; set; }
        public int UkupnoOcjena { get; set; }
        public int UkupnoIzostanaka { get; set; }
        public int NeopravdaniIzostanci { get; set; }

        public string ProsjekRazredaText => ProsjekRazreda > 0 ? ProsjekRazreda.ToString("F2") : "Nema ocjena";
        public string ProsjekRazredaClass => ProsjekRazreda >= 4.0 ? "text-success" :
                                            ProsjekRazreda >= 3.0 ? "text-warning" :
                                            ProsjekRazreda >= 2.0 ? "text-danger" : "text-muted";
    }

    public class RazredDetaljeStatistike
    {
        public RazredStatistike Osnovne { get; set; } = new();
        public Korisnik? NajboljiUcenik { get; set; }
        public Korisnik? UcenikSaNajviseIzostanaka { get; set; }
        public int BrojUcenikaVrloDobro { get; set; } // >= 4.5
        public int BrojUcenikaDobro { get; set; } // 3.5 - 4.5
        public int BrojUcenikaNedovoljno { get; set; } // < 2.0
    }

    public class OcjenePoPremetima
    {
        public Predmet Predmet { get; set; } = new();
        public double Prosjek { get; set; }
        public int BrojOcjena { get; set; }
        public Dictionary<int, int> Distribucija { get; set; } = new();

        public string ProsjekText => Prosjek > 0 ? Prosjek.ToString("F2") : "Nema ocjena";
        public string ProsjekClass => Prosjek >= 4.0 ? "text-success" :
                                     Prosjek >= 3.0 ? "text-warning" :
                                     Prosjek >= 2.0 ? "text-danger" : "text-muted";
    }
}