using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VolunteerHub.Constants;
using VolunteerHub.Data;
using VolunteerHub.Models;
using VolunteerHub.Services;
using VolunteerHub.ViewModels;

namespace VolunteerHub.Controllers
{
    public class VolunteerActionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITimeService _timeService;

        public VolunteerActionsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ITimeService timeService)
        {
            _context = context;
            _userManager = userManager;
            _timeService = timeService;
        }


        // -------------------------------------------------
        // PUBLIC ACTION LIST
        // -------------------------------------------------

        [AllowAnonymous]
        public async Task<IActionResult> Index(
            string? searchString,
            string? category,
            int? countryId,
            int? cityId,
            DateTime? date)
        {
            var actions = _context.VolunteerActions
                .Include(a => a.City)
                    .ThenInclude(c => c!.Country)
                .Where(a =>
                    a.Status == VolunteerActionStatuses.Published ||
                    a.Status == VolunteerActionStatuses.InProgress)
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(searchString))
            {
                actions = actions.Where(a =>
                    a.Title.Contains(searchString) ||
                    a.Description.Contains(searchString));
            }


            if (!string.IsNullOrWhiteSpace(category))
            {
                actions = actions.Where(a =>
                    a.Category == category);
            }


            if (countryId.HasValue)
            {
                actions = actions.Where(a =>
                    a.City != null &&
                    a.City.CountryId == countryId.Value);
            }


            if (cityId.HasValue)
            {
                actions = actions.Where(a =>
                    a.CityId == cityId.Value);
            }


            if (date.HasValue)
            {
                actions = actions.Where(a =>
                    a.StartDate.Date == date.Value.Date);
            }


            var model =
                new VolunteerActionIndexViewModel
                {
                    SearchString = searchString,
                    Category = category,
                    CountryId = countryId,
                    CityId = cityId,
                    Date = date,

                    Actions = await actions
                        .OrderBy(a => a.StartDate)
                        .ToListAsync(),

                    Categories =
                        new List<SelectListItem>
                        {
                            new SelectListItem
                            {
                                Value = VolunteerActionCategories.Environment,
                                Text = "Environment"
                            },

                            new SelectListItem
                            {
                                Value = VolunteerActionCategories.Animals,
                                Text = "Animal Welfare"
                            },

                            new SelectListItem
                            {
                                Value = VolunteerActionCategories.Social,
                                Text = "Community & Social Support"
                            },

                            new SelectListItem
                            {
                                Value = VolunteerActionCategories.Education,
                                Text = "Education"
                            },

                            new SelectListItem
                            {
                                Value = VolunteerActionCategories.Health,
                                Text = "Health & Wellbeing"
                            },

                            new SelectListItem
                            {
                                Value = VolunteerActionCategories.Other,
                                Text = "Other"
                            }
                        },

                    Countries =
                        await _context.Countries
                            .OrderBy(c => c.Name)
                            .Select(c =>
                                new SelectListItem
                                {
                                    Value = c.Id.ToString(),
                                    Text = c.Name
                                })
                            .ToListAsync()
                };


            if (countryId.HasValue)
            {
                model.Cities =
                    await _context.Cities
                        .Where(c =>
                            c.CountryId == countryId.Value)
                        .OrderBy(c => c.Name)
                        .Select(c =>
                            new SelectListItem
                            {
                                Value = c.Id.ToString(),
                                Text = c.Name
                            })
                        .ToListAsync();
            }


            return View(model);
        }


        // -------------------------------------------------
        // DETAILS
        // -------------------------------------------------

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var volunteerAction =
                await _context.VolunteerActions
                    .Include(a => a.Organizer)
                    .Include(a => a.City)
                        .ThenInclude(c => c!.Country)
                    .Include(a => a.Shifts)
                    .FirstOrDefaultAsync(a =>
                        a.Id == id);


            if (volunteerAction == null)
            {
                return NotFound();
            }


            bool isPublic =
                volunteerAction.Status ==
                    VolunteerActionStatuses.Published ||
                volunteerAction.Status ==
                    VolunteerActionStatuses.InProgress;


            bool isOwner =
                User.Identity != null &&
                User.Identity.IsAuthenticated &&
                volunteerAction.OrganizerId ==
                _userManager.GetUserId(User);


            if (!isPublic && !isOwner)
            {
                return NotFound();
            }


            return View(volunteerAction);
        }


        // -------------------------------------------------
        // ORGANIZER ACTIONS
        // -------------------------------------------------

        [Authorize(Roles = UserRoles.Organizer)]
        public async Task<IActionResult> MyActions()
        {
            var userId =
                _userManager.GetUserId(User);


            var actions =
                await _context.VolunteerActions
                    .Include(a => a.City)
                        .ThenInclude(c => c!.Country)
                    .Where(a =>
                        a.OrganizerId == userId)
                    .OrderByDescending(a => a.Id)
                    .ToListAsync();


            return View(actions);
        }


        // -------------------------------------------------
        // CREATE GET
        // -------------------------------------------------

        [Authorize(Roles = UserRoles.Organizer)]
        public async Task<IActionResult> Create()
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }


            var organizer =
                await _context.Users
                    .Include(u => u.City)
                        .ThenInclude(c => c!.Country)
                    .FirstOrDefaultAsync(u =>
                        u.Id == userId);


            DateTime localToday =
                _timeService.UtcNow.Date;


            var timeZoneId =
                organizer?.City?
                    .Country?
                    .TimeZoneId;


            if (!string.IsNullOrWhiteSpace(
                timeZoneId))
            {
                localToday =
                    _timeService.GetCurrentLocalTime(
                        timeZoneId)
                    .Date;
            }


            var model =
                new VolunteerActionFormViewModel
                {
                    StartDate =
                        localToday.AddDays(7)
                            .AddHours(9),

                    EndDate =
                        localToday.AddDays(7)
                            .AddHours(17),

                    ApplicationDeadline =
                        localToday.AddDays(6)
                            .AddHours(18)
                };


            await LoadFormOptionsAsync(model);

            return View(model);
        }


        // -------------------------------------------------
        // CREATE POST
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Organizer)]
        public async Task<IActionResult> Create(
            VolunteerActionFormViewModel model)
        {
            await ValidateLocationAsync(model);

            ValidateDates(model);


            if (!ModelState.IsValid)
            {
                await LoadFormOptionsAsync(model);

                return View(model);
            }


            var volunteerAction =
                new VolunteerAction
                {
                    Title =
                        model.Title.Trim(),

                    Description =
                        model.Description.Trim(),

                    Category =
                        model.Category,

                    CityId =
                        model.CityId,

                    StartDate =
                        model.StartDate,

                    EndDate =
                        model.EndDate,

                    ApplicationDeadline =
                        model.ApplicationDeadline,

                    ContactInfo =
                        model.ContactInfo.Trim(),

                    MinimumAge =
                        model.MinimumAge,

                    Instructions =
                        model.Instructions,

                    OrganizerId =
                        _userManager.GetUserId(User)!,

                    Status =
                        VolunteerActionStatuses.Draft
                };


            _context.VolunteerActions.Add(
                volunteerAction);


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "The volunteer action was created successfully.";


            return RedirectToAction(
                nameof(MyActions));
        }


        // -------------------------------------------------
        // EDIT GET
        // Draft / Published only
        // -------------------------------------------------

        [Authorize(Roles = UserRoles.Organizer)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var volunteerAction =
                await _context.VolunteerActions
                    .Include(a => a.City)
                    .FirstOrDefaultAsync(a =>
                        a.Id == id);


            if (volunteerAction == null)
            {
                return NotFound();
            }


            var userId =
                _userManager.GetUserId(User);


            if (volunteerAction.OrganizerId != userId)
            {
                return Forbid();
            }


            if (!CanEditAction(
                volunteerAction.Status))
            {
                TempData["Error"] =
                    "Only draft or published opportunities can be edited.";

                return RedirectToAction(
                    nameof(MyActions));
            }


            var model =
                new VolunteerActionFormViewModel
                {
                    Id =
                        volunteerAction.Id,

                    Title =
                        volunteerAction.Title,

                    Description =
                        volunteerAction.Description,

                    Category =
                        volunteerAction.Category,

                    CountryId =
                        volunteerAction.City?.CountryId,

                    CityId =
                        volunteerAction.CityId,

                    StartDate =
                        volunteerAction.StartDate,

                    EndDate =
                        volunteerAction.EndDate,

                    ApplicationDeadline =
                        volunteerAction.ApplicationDeadline,

                    ContactInfo =
                        volunteerAction.ContactInfo,

                    MinimumAge =
                        volunteerAction.MinimumAge,

                    Instructions =
                        volunteerAction.Instructions
                };


            await LoadFormOptionsAsync(model);

            return View(model);
        }


        // -------------------------------------------------
        // EDIT POST
        // Draft / Published only
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Organizer)]
        public async Task<IActionResult> Edit(
            int id,
            VolunteerActionFormViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }


            var volunteerAction =
                await _context.VolunteerActions
                    .FirstOrDefaultAsync(a =>
                        a.Id == id);


            if (volunteerAction == null)
            {
                return NotFound();
            }


            var userId =
                _userManager.GetUserId(User);


            if (volunteerAction.OrganizerId != userId)
            {
                return Forbid();
            }


            if (!CanEditAction(
                volunteerAction.Status))
            {
                TempData["Error"] =
                    "Only draft or published opportunities can be edited.";

                return RedirectToAction(
                    nameof(MyActions));
            }


            await ValidateLocationAsync(model);

            ValidateDates(model);

            await ValidateScheduleChangesAsync(
                volunteerAction,
                model);


            if (!ModelState.IsValid)
            {
                await LoadFormOptionsAsync(model);

                return View(model);
            }


            volunteerAction.Title =
                model.Title.Trim();

            volunteerAction.Description =
                model.Description.Trim();

            volunteerAction.Category =
                model.Category;

            volunteerAction.CityId =
                model.CityId;

            volunteerAction.StartDate =
                model.StartDate;

            volunteerAction.EndDate =
                model.EndDate;

            volunteerAction.ApplicationDeadline =
                model.ApplicationDeadline;

            volunteerAction.ContactInfo =
                model.ContactInfo.Trim();

            volunteerAction.MinimumAge =
                model.MinimumAge;

            volunteerAction.Instructions =
                model.Instructions;


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "The volunteer action was updated successfully.";


            return RedirectToAction(
                nameof(MyActions));
        }


        // -------------------------------------------------
        // PUBLISH
        // Draft → Published
        // Requires at least one shift
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Organizer)]
        public async Task<IActionResult> Publish(int id)
        {
            var volunteerAction =
                await GetOwnedActionAsync(id);


            if (volunteerAction == null)
            {
                return NotFound();
            }


            if (volunteerAction.Status !=
                VolunteerActionStatuses.Draft)
            {
                TempData["Error"] =
                    "Only draft opportunities can be published.";

                return RedirectToAction(
                    nameof(MyActions));
            }


            bool hasShifts =
                await _context.Shifts
                    .AnyAsync(s =>
                        s.VolunteerActionId ==
                        volunteerAction.Id);


            if (!hasShifts)
            {
                TempData["Error"] =
                    "This opportunity cannot be published until at least one shift has been created.";

                return RedirectToAction(
                    nameof(MyActions));
            }


            volunteerAction.Status =
                VolunteerActionStatuses.Published;


            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"\"{volunteerAction.Title}\" was published successfully.";


            return RedirectToAction(
                nameof(MyActions));
        }


        // -------------------------------------------------
        // START
        // Published → InProgress
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Organizer)]
        public async Task<IActionResult> Start(int id)
        {
            var volunteerAction =
                await GetOwnedActionAsync(id);


            if (volunteerAction == null)
            {
                return NotFound();
            }


            if (volunteerAction.Status !=
                VolunteerActionStatuses.Published)
            {
                TempData["Error"] =
                    "Only published opportunities can be started.";

                return RedirectToAction(
                    nameof(MyActions));
            }


            if (volunteerAction.City?.Country == null ||
                string.IsNullOrWhiteSpace(
                    volunteerAction.City.Country.TimeZoneId))
            {
                TempData["Error"] =
                    "The opportunity time zone could not be determined.";

                return RedirectToAction(
                    nameof(MyActions));
            }


            var now =
                _timeService.GetCurrentLocalTime(
                    volunteerAction.City.Country.TimeZoneId);


            if (now < volunteerAction.StartDate)
            {
                TempData["Error"] =
                    $"This opportunity cannot be started before " +
                    $"{volunteerAction.StartDate:dd MMM yyyy, HH:mm}.";

                return RedirectToAction(
                    nameof(MyActions));
            }


            if (now >= volunteerAction.EndDate)
            {
                TempData["Error"] =
                    "This opportunity has already ended and cannot be started.";

                return RedirectToAction(
                    nameof(MyActions));
            }


            volunteerAction.Status =
                VolunteerActionStatuses.InProgress;


            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"\"{volunteerAction.Title}\" is now in progress.";


            return RedirectToAction(
                nameof(MyActions));
        }


        // -------------------------------------------------
        // COMPLETE
        // InProgress → Completed
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Organizer)]
        public async Task<IActionResult> Complete(int id)
        {
            var volunteerAction =
                await GetOwnedActionAsync(id);


            if (volunteerAction == null)
            {
                return NotFound();
            }


            if (volunteerAction.Status !=
                VolunteerActionStatuses.InProgress)
            {
                TempData["Error"] =
                    "Only opportunities that are in progress can be completed.";

                return RedirectToAction(
                    nameof(MyActions));
            }


            volunteerAction.Status =
                VolunteerActionStatuses.Completed;


            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"\"{volunteerAction.Title}\" was completed successfully.";


            return RedirectToAction(
                nameof(MyActions));
        }


        // -------------------------------------------------
        // CANCEL
        // Published / InProgress → Cancelled
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Organizer)]
        public async Task<IActionResult> Cancel(int id)
        {
            var volunteerAction =
                await GetOwnedActionAsync(id);


            if (volunteerAction == null)
            {
                return NotFound();
            }


            bool canCancel =
                volunteerAction.Status ==
                    VolunteerActionStatuses.Published ||
                volunteerAction.Status ==
                    VolunteerActionStatuses.InProgress;


            if (!canCancel)
            {
                TempData["Error"] =
                    "Only published or in-progress opportunities can be cancelled.";

                return RedirectToAction(
                    nameof(MyActions));
            }


            volunteerAction.Status =
                VolunteerActionStatuses.Cancelled;


            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"\"{volunteerAction.Title}\" was cancelled successfully.";


            return RedirectToAction(
                nameof(MyActions));
        }


        // -------------------------------------------------
        // FORM OPTIONS
        // -------------------------------------------------

        private async Task LoadFormOptionsAsync(
            VolunteerActionFormViewModel model)
        {
            model.Countries =
                await _context.Countries
                    .OrderBy(c => c.Name)
                    .Select(c =>
                        new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.Name
                        })
                    .ToListAsync();


            if (model.CountryId.HasValue)
            {
                model.Cities =
                    await _context.Cities
                        .Where(c =>
                            c.CountryId ==
                            model.CountryId.Value)
                        .OrderBy(c => c.Name)
                        .Select(c =>
                            new SelectListItem
                            {
                                Value = c.Id.ToString(),
                                Text = c.Name
                            })
                        .ToListAsync();
            }
            else
            {
                model.Cities =
                    new List<SelectListItem>();
            }


            model.Categories =
                new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Value = VolunteerActionCategories.Environment,
                        Text = "Environment"
                    },

                    new SelectListItem
                    {
                        Value = VolunteerActionCategories.Animals,
                        Text = "Animal Welfare"
                    },

                    new SelectListItem
                    {
                        Value = VolunteerActionCategories.Social,
                        Text = "Community & Social Support"
                    },

                    new SelectListItem
                    {
                        Value = VolunteerActionCategories.Education,
                        Text = "Education"
                    },

                    new SelectListItem
                    {
                        Value = VolunteerActionCategories.Health,
                        Text = "Health & Wellbeing"
                    },

                    new SelectListItem
                    {
                        Value = VolunteerActionCategories.Other,
                        Text = "Other"
                    }
                };
        }


        // -------------------------------------------------
        // LOCATION VALIDATION
        // -------------------------------------------------

        private async Task ValidateLocationAsync(
            VolunteerActionFormViewModel model)
        {
            if (!model.CountryId.HasValue ||
                !model.CityId.HasValue)
            {
                return;
            }


            bool validCity =
                await _context.Cities
                    .AnyAsync(c =>
                        c.Id == model.CityId.Value &&
                        c.CountryId ==
                        model.CountryId.Value);


            if (!validCity)
            {
                ModelState.AddModelError(
                    nameof(model.CityId),
                    "Please select a valid city for the selected country.");
            }
        }


        // -------------------------------------------------
        // DATE VALIDATION
        // -------------------------------------------------

        private void ValidateDates(
            VolunteerActionFormViewModel model)
        {
            if (model.EndDate <=
                model.StartDate)
            {
                ModelState.AddModelError(
                    nameof(model.EndDate),
                    "The end date must be after the start date.");
            }


            if (model.ApplicationDeadline >=
                model.StartDate)
            {
                ModelState.AddModelError(
                    nameof(model.ApplicationDeadline),
                    "The application deadline must be before the start date.");
            }
        }


        // -------------------------------------------------
        // EXISTING SHIFT / APPLICATION VALIDATION
        // -------------------------------------------------

        private async Task ValidateScheduleChangesAsync(
            VolunteerAction volunteerAction,
            VolunteerActionFormViewModel model)
        {
            bool scheduleChanged =
                volunteerAction.StartDate !=
                    model.StartDate ||
                volunteerAction.EndDate !=
                    model.EndDate;


            if (!scheduleChanged)
            {
                return;
            }


            bool hasApplications =
                await _context.ParticipationRequests
                    .AnyAsync(r =>
                        r.Shift.VolunteerActionId ==
                        volunteerAction.Id);


            if (volunteerAction.Status ==
                    VolunteerActionStatuses.Published &&
                hasApplications)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The start and end dates cannot be changed because volunteers have already applied to this opportunity.");

                return;
            }


            bool hasShiftOutsideNewSchedule =
                await _context.Shifts
                    .AnyAsync(s =>
                        s.VolunteerActionId ==
                            volunteerAction.Id &&
                        (
                            s.StartTime <
                                model.StartDate ||
                            s.EndTime >
                                model.EndDate
                        ));


            if (hasShiftOutsideNewSchedule)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The new opportunity schedule must include all existing shifts. Edit the shifts first or choose a wider date range.");
            }
        }


        // -------------------------------------------------
        // EDIT STATUS CHECK
        // -------------------------------------------------

        private static bool CanEditAction(
            string status)
        {
            return
                status ==
                    VolunteerActionStatuses.Draft ||
                status ==
                    VolunteerActionStatuses.Published;
        }


        // -------------------------------------------------
        // OWNERSHIP CHECK
        // -------------------------------------------------

        private async Task<VolunteerAction?>
            GetOwnedActionAsync(int id)
        {
            var userId =
                _userManager.GetUserId(User);


            return await _context.VolunteerActions
                .Include(a => a.City)
                    .ThenInclude(c => c!.Country)
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    a.OrganizerId == userId);
        }
    }
}