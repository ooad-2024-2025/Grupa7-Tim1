namespace eDnevnik.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int BrojUcenika { get; set; }
        public int BrojNastavnika { get; set; }
        public int BrojRoditelja { get; set; }
        public int BrojRazreda { get; set; }
        public int BrojPredmeta { get; set; }

        public List<KorisnikViewModel> Korisnici { get; set; }
    }
}
