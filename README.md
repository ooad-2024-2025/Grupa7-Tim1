# eDnevnik – Informacioni sistem za obrazovne ustanove

---

## O projektu

**eDnevnik** je sveobuhvatan informacioni sistem dizajniran za digitalizaciju administracije u obrazovnim institucijama. Omogućava:

- evidenciju nastavnika, učenika i predmeta (CRUD operacije),
- generisanje izvještaja o ocjenama, prisustvu i vladanju u Excel formatu,
- pregled i kreiranje nadolazećih aktivnosti,
- slanje automatskih obavijesti učenicima i roditeljima putem emaila,
- statistički prikaz uspjeha učenika (prosjek, izostanci, vladanje),
- evidenciju održanih časova i izostanaka učenika,
- direktnu komunikaciju među akterima putem chata i obavijesti,
- pregled rasporeda časova s opcijama filtriranja.

Cilj sistema je da poveća transparentnost i efikasnost obrazovnog procesa te zamijeni fizički dnevnik modernim digitalnim rješenjem.

---

## Online pristup

Aplikacija je hostovana na sljedećem linku:  
🔗 [http://mirnes-001-site1.anytempurl.com](http://mirnes-001-site1.anytempurl.com)

---

## Testni korisnici

| Uloga    | Email                     | Lozinka       |
|---------------------|---------------------------|---------------|
| Admin               | admin@ednevnik.com        | Admin123!     |
| Nastavnik/Razrednik | nastavnikprvi@gmail.com   | Nastavnik123! |
| Ucenik              | ucenikprvi@gmail.com      | Ucenik123!    |
| Roditelj            | ebegic5@etf.unsa.ba       | Student123!   |

---

## Konekcijski string

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=SQL6031.site4now.net;Initial Catalog=db_aba372_ednevnik;User Id=db_aba372_ednevnik_admin;Password=japan2023"
}
