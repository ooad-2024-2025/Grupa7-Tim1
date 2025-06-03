using eDnevnik.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;

@model List<eDnevnik.Models.PredmetRazred>

@{
    ViewData["Title"] = "Predmeti u razredu";
string nazivRazreda = ViewBag.RazredNaziv ?? "Nepoznat razred";
}

< h2 class= "text-center mb-4" > Predmeti za razred: @nazivRazreda </ h2 >

@if(!Model.Any())
{
    < div class= "alert alert-info text-center" >
        Nema dodijeljenih predmeta za ovaj razred.
    </div>
}
else
{
    < table class= "table table-bordered table-striped" >
        < thead class= "table-primary" >
            < tr class= "text-center" >
                < th > Predmet </ th >
                < th > Nastavnik </ th >
            </ tr >
        </ thead >
        < tbody class= "text-center" >
            @foreach(var pr in Model)
            {
                < tr >
                    < td > @pr.Predmet.Naziv </ td >
                    < td >@(pr.Nastavnik != null ? pr.Nastavnik.Ime + " " + pr.Nastavnik.Prezime : "Nije dodijeljen") </ td >
                </ tr >
            }
        </ tbody >
    </ table >
}

< div class= "text-center mt-4" >
    < a class= "btn btn-secondary" asp - action = "Index" > Nazad na pregled razreda</a>
</div>
