namespace eDnevnik.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class RedirectController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Ako je logovan, idi na početnu (npr. Dashboard, Home, Razredi...)
                return RedirectToAction("Index", "Home");
            }

            // Inače idi na login
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }
    }

}
