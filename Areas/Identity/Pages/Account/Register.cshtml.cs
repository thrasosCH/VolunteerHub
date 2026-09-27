using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VolunteerHub.Constants;
using VolunteerHub.Data;
using VolunteerHub.Models;

namespace VolunteerHub.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<SelectListItem> Countries { get; set; } = new();

        public List<SelectListItem> Cities { get; set; } = new();

        public List<SelectListItem> CountryOptions => Countries;

        public List<SelectListItem> CityOptions => Cities;

        public string? ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
            = new List<AuthenticationScheme>();


        // -------------------------------------------------
        // GET
        // -------------------------------------------------

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            ExternalLogins =
                (await _signInManager
                    .GetExternalAuthenticationSchemesAsync())
                .ToList();

            await LoadCountriesAsync();
        }


        // -------------------------------------------------
        // POST
        // -------------------------------------------------

        public async Task<IActionResult> OnPostAsync(
            string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ReturnUrl = returnUrl;

            ExternalLogins =
                (await _signInManager
                    .GetExternalAuthenticationSchemesAsync())
                .ToList();

            await LoadCountriesAsync();

            await LoadCitiesAsync(Input.CountryId);


            // Validate role

            if (Input.Role != UserRoles.Volunteer &&
                Input.Role != UserRoles.Organizer)
            {
                ModelState.AddModelError(
                    "Input.Role",
                    "Please select a valid account type.");
            }


            // Validate city belongs to selected country

            if (Input.CountryId.HasValue &&
                Input.CityId.HasValue)
            {
                var cityExists =
                    await _context.Cities.AnyAsync(c =>
                        c.Id == Input.CityId.Value &&
                        c.CountryId == Input.CountryId.Value);

                if (!cityExists)
                {
                    ModelState.AddModelError(
                        "Input.CityId",
                        "Please select a valid city for the selected country.");
                }
            }


            if (!ModelState.IsValid)
            {
                return Page();
            }


            // Create user

            var user = new ApplicationUser
            {
                UserName = Input.Email.Trim(),

                Email = Input.Email.Trim(),

                EmailConfirmed = true,

                FullName = Input.FullName.Trim(),

                PhoneNumber =
                    string.IsNullOrWhiteSpace(Input.PhoneNumber)
                        ? null
                        : Input.PhoneNumber.Trim(),

                Bio =
                    Input.Bio?.Trim()
                    ?? string.Empty,

                Skills =
                    Input.Skills?.Trim()
                    ?? string.Empty,

                CityId =
                    Input.CityId!.Value,

                RegistrationDate =
                    DateTime.UtcNow
            };


            var result =
                await _userManager.CreateAsync(
                    user,
                    Input.Password);


            if (result.Succeeded)
            {
                var roleResult =
                    await _userManager.AddToRoleAsync(
                        user,
                        Input.Role);


                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }


                    await _userManager.DeleteAsync(user);


                    return Page();
                }


                await _signInManager.SignInAsync(
                    user,
                    isPersistent: false);


                return LocalRedirect(returnUrl);
            }


            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }


            return Page();
        }


        // -------------------------------------------------
        // COUNTRIES
        // -------------------------------------------------

        private async Task LoadCountriesAsync()
        {
            Countries =
                await _context.Countries
                    .OrderBy(c => c.Name)
                    .Select(c =>
                        new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.Name
                        })
                    .ToListAsync();
        }


        // -------------------------------------------------
        // CITIES
        // -------------------------------------------------

        private async Task LoadCitiesAsync(
            int? countryId)
        {
            if (!countryId.HasValue)
            {
                Cities =
                    new List<SelectListItem>();

                return;
            }


            Cities =
                await _context.Cities
                    .Where(c =>
                        c.CountryId ==
                        countryId.Value)
                    .OrderBy(c =>
                        c.Name)
                    .Select(c =>
                        new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.Name
                        })
                    .ToListAsync();
        }


        // -------------------------------------------------
        // INPUT MODEL
        // -------------------------------------------------

        public class InputModel
        {
            [Required]
            [StringLength(
                150,
                MinimumLength = 2)]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }
                = string.Empty;


            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
                = string.Empty;


            [Phone]
            [Display(Name = "Phone Number")]
            public string? PhoneNumber { get; set; }


            [Required(
                ErrorMessage =
                    "Please select a country.")]
            [Display(Name = "Country")]
            public int? CountryId { get; set; }


            [Required(
                ErrorMessage =
                    "Please select a city.")]
            [Display(Name = "City")]
            public int? CityId { get; set; }


            [StringLength(500)]
            [Display(Name = "About Me")]
            public string? Bio { get; set; }


            [StringLength(500)]
            [Display(Name = "Skills & Interests")]
            public string? Skills { get; set; }


            [Required]
            [Display(Name = "Account Type")]
            public string Role { get; set; }
                = UserRoles.Volunteer;


            [Required]
            [StringLength(
                100,
                ErrorMessage =
                    "The password must be at least {2} characters long.",
                MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }
                = string.Empty;


            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare(
                "Password",
                ErrorMessage =
                    "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
                = string.Empty;
        }
    }
}