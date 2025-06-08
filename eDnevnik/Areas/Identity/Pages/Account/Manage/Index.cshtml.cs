using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using eDnevnik.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace eDnevnik.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly SignInManager<Korisnik> _signInManager;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(UserManager<Korisnik> userManager, SignInManager<Korisnik> signInManager, ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public string Username { get; set; }

        public string Telefon { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public ChangePasswordInputModel ChangePasswordInput { get; set; } = new ChangePasswordInputModel();

        private string TranslatePasswordError(string errorCode)
        {
            return errorCode switch
            {
                "PasswordTooShort" => "Lozinka je prekratka.",
                "PasswordRequiresNonAlphanumeric" => "Lozinka mora sadržavati barem jedan specijalni znak.",
                "PasswordRequiresLower" => "Lozinka mora sadržavati barem jedno malo slovo.",
                "PasswordRequiresUpper" => "Lozinka mora sadržavati barem jedno veliko slovo.",
                "PasswordRequiresDigit" => "Lozinka mora sadržavati barem jednu cifru.",
                _ => "Greška prilikom unosa lozinke."
            };
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Broj telefona je obavezan.")]
            [RegularExpression(@"^\d{3,15}$", ErrorMessage = "Unesite validan broj telefona.")]
            [Display(Name = "Broj telefona")]
            public string Telefon { get; set; }
        }

        public class ChangePasswordInputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Trenutna lozinka")]
            public string OldPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "Lozinka mora imati minimalno {2} i maksimalno {1} karaktera.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Nova lozinka")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Potvrdite novu lozinku")]
            [Compare("NewPassword", ErrorMessage = "Nova lozinka i potvrda lozinke se ne podudaraju.")]
            public string ConfirmPassword { get; set; }
        }

        private async Task LoadAsync(Korisnik user)
        {
            var userName = await _userManager.GetUserNameAsync(user);

            Username = userName;
            Telefon = user.Telefon;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeTelefonAsync(InputModel Input)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation("DEBUG: OnPostChangeTelefonAsync called.");
            _logger.LogInformation("DEBUG: Input.Telefon = '{InputTelefon}'", Input.Telefon);

            if (!ModelState.IsValid)
            {
                _logger.LogInformation("DEBUG: ModelState nije validan.");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogInformation("Error: {ErrorMessage}", error.ErrorMessage);
                }

                StatusMessage = "Greška: Uneseni broj telefona nije validan.";
                await LoadAsync(user);
                return Page();
            }

            if (!string.Equals(Input.Telefon, user.Telefon, StringComparison.Ordinal))
            {
                user.Telefon = Input.Telefon;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Došlo je do greške prilikom postavljanja broja telefona.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Broj telefona je uspješno ažuriran.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync(ChangePasswordInputModel ChangePasswordInput)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation("DEBUG: OnPostChangePasswordAsync called.");

            if (!ModelState.IsValid)
            {
                _logger.LogInformation("DEBUG: ModelState nije validan.");
                await LoadAsync(user);
                return Page();
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(
                user,
                ChangePasswordInput.OldPassword,
                ChangePasswordInput.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    _logger.LogInformation("Error: {ErrorMessage} (Code: {ErrorCode})", error.Description, error.Code);

                    if (error.Code == "PasswordMismatch")
                    {
                        ModelState.AddModelError("ChangePasswordInput.OldPassword", "Netačna trenutna lozinka.");
                    }
                    else if (error.Code == "PasswordTooShort"
                          || error.Code == "PasswordRequiresNonAlphanumeric"
                          || error.Code == "PasswordRequiresLower"
                          || error.Code == "PasswordRequiresUpper"
                          || error.Code == "PasswordRequiresDigit")
                    {
                        // Greške vezane za NOVU lozinku → prikazati ispod polja Nova lozinka
                        ModelState.AddModelError("ChangePasswordInput.NewPassword", TranslatePasswordError(error.Code));
                    }
                    else
                    {
                        // Sve ostale eventualne greške → možeš globalno ili po potrebi
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                await LoadAsync(user);
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Lozinka je uspješno promijenjena.";
            return RedirectToPage();
        }


    }
}
