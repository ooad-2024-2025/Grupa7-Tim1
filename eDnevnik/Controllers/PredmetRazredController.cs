using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using eDnevnik.Data;
using eDnevnik.Models;

[Authorize(Roles = "Administrator")]
public class PredmetRazredController : Controller
{
    private readonly ApplicationDbContext _context;

    public PredmetRazredController(ApplicationDbContext context)
    {
        _context = context;
    }


    [HttpPost]
    public IActionResult Dodaj(PredmetRazred model)
    {
        if (!ModelState.IsValid)
        {
        if (!postoji)
        {
            _context.PredmetRazred.Add(model);
            _context.SaveChanges();
        }

        return RedirectToAction("DetaljiPredmeti", "Razredi", new { id = model.RazredId });
    }
}
