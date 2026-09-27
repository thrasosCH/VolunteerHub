using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerHub.Data;
using VolunteerHub.Models;

namespace VolunteerHub.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // -------------------------------------------------
        // MY PROFILE
        // -------------------------------------------------

        public async Task<IActionResult> Index()
        {
            var userId =
                _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }


            var user =
                await _context.Users
                    .Include(u => u.City)
                        .ThenInclude(c => c!.Country)
                    .FirstOrDefaultAsync(u =>
                        u.Id == userId);


            if (user == null)
            {
                return NotFound();
            }


            return View(user);
        }


        // -------------------------------------------------
        // EDIT PROFILE - GET
        // -------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId =
                _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }


            var user =
                await _context.Users
                    .Include(u => u.City)
                        .ThenInclude(c => c!.Country)
                    .FirstOrDefaultAsync(u =>
                        u.Id == userId);


            if (user == null)
            {
                return NotFound();
            }


            var selectedCountryId =
                user.City?.CountryId ?? 0;


            await LoadLocationsAsync(
                selectedCountryId);


            ViewBag.SelectedCountryId =
                selectedCountryId;


            return View(user);
        }


        // -------------------------------------------------
        // EDIT PROFILE - POST
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string fullName,
            string? phoneNumber,
            int countryId,
            int cityId,
            string? bio,
            string? skills)
        {
            var userId =
                _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }


            var user =
                await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.Id == userId);


            if (user == null)
            {
                return NotFound();
            }


            // -------------------------------------------------
            // VALIDATE FULL NAME
            // -------------------------------------------------

            if (string.IsNullOrWhiteSpace(fullName))
            {
                ModelState.AddModelError(
                    "FullName",
                    "Full name is required.");
            }
            else if (fullName.Trim().Length > 150)
            {
                ModelState.AddModelError(
                    "FullName",
                    "Full name cannot exceed 150 characters.");
            }


            // -------------------------------------------------
            // VALIDATE BIO
            // -------------------------------------------------

            if (!string.IsNullOrWhiteSpace(bio) &&
                bio.Trim().Length > 500)
            {
                ModelState.AddModelError(
                    "Bio",
                    "Bio cannot exceed 500 characters.");
            }


            // -------------------------------------------------
            // VALIDATE SKILLS
            // -------------------------------------------------

            if (!string.IsNullOrWhiteSpace(skills) &&
                skills.Trim().Length > 500)
            {
                ModelState.AddModelError(
                    "Skills",
                    "Skills and interests cannot exceed 500 characters.");
            }


            // -------------------------------------------------
            // VALIDATE COUNTRY + CITY
            // -------------------------------------------------

            var selectedCity =
                await _context.Cities
                    .Include(c => c.Country)
                    .FirstOrDefaultAsync(c =>
                        c.Id == cityId &&
                        c.CountryId == countryId);


            if (selectedCity == null)
            {
                ModelState.AddModelError(
                    "CityId",
                    "Please select a valid city for the selected country.");
            }


            // -------------------------------------------------
            // RETURN VIEW IF VALIDATION FAILED
            // -------------------------------------------------

            if (!ModelState.IsValid)
            {
                user.FullName =
                    fullName?.Trim()
                    ?? string.Empty;

                user.PhoneNumber =
                    string.IsNullOrWhiteSpace(phoneNumber)
                        ? null
                        : phoneNumber.Trim();

                user.Bio =
                    bio?.Trim()
                    ?? string.Empty;

                user.Skills =
                    skills?.Trim()
                    ?? string.Empty;

                user.CityId =
                    cityId > 0
                        ? cityId
                        : null;


                if (selectedCity != null)
                {
                    user.City = selectedCity;
                }


                await LoadLocationsAsync(
                    countryId);


                ViewBag.SelectedCountryId =
                    countryId;


                return View(user);
            }


            // -------------------------------------------------
            // UPDATE USER
            // -------------------------------------------------

            user.FullName =
                fullName.Trim();

            user.PhoneNumber =
                string.IsNullOrWhiteSpace(phoneNumber)
                    ? null
                    : phoneNumber.Trim();

            user.Bio =
                bio?.Trim()
                ?? string.Empty;

            user.Skills =
                skills?.Trim()
                ?? string.Empty;

            user.CityId =
                cityId;


            var result =
                await _userManager.UpdateAsync(user);


            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }


                user.City =
                    selectedCity;


                await LoadLocationsAsync(
                    countryId);


                ViewBag.SelectedCountryId =
                    countryId;


                return View(user);
            }


            TempData["Success"] =
                "Your profile was updated successfully.";


            return RedirectToAction(
                nameof(Index));
        }


        // -------------------------------------------------
        // LOAD COUNTRY + CITY LISTS
        // -------------------------------------------------

        private async Task LoadLocationsAsync(
            int countryId)
        {
            ViewBag.Countries =
                await _context.Countries
                    .OrderBy(c => c.Name)
                    .ToListAsync();


            if (countryId > 0)
            {
                ViewBag.Cities =
                    await _context.Cities
                        .Where(c =>
                            c.CountryId == countryId)
                        .OrderBy(c => c.Name)
                        .ToListAsync();
            }
            else
            {
                ViewBag.Cities =
                    new List<City>();
            }
        }
    }
}