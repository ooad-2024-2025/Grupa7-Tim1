namespace eDnevnik.Data.@enum
{
    public enum TipAktivnosti
    {
        Test = 1,
        Zadaca = 2,
        Takmicenje = 3,
        SkolarskiDogadjaj = 4,
        Prezentacija = 5,
        Ekskurzija = 6
    }

    public enum PrioritetAktivnosti
    {
        Nizak = 1,      // 1 dan prije
        Srednji = 2,    // 3 dana prije  
        Visok = 3       // Odmah
    }

    public enum StatusObavjestenja
    {
        Čeka = 1,
        Poslano = 2,
        Greška = 3,
        Preskočeno = 4
    }
}