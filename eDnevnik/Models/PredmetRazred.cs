namespace eDnevnik.Models
{
    public class PredmetRazred
    {
        public int Id { get; set; }

        public int RazredId { get; set; }
        public Razred Razred { get; set; }

        public int PredmetId { get; set; }
        public Predmet Predmet { get; set; }
    }

}
