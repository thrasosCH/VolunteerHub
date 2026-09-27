using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerHub.Constants;
using VolunteerHub.Data;
using VolunteerHub.Models;

namespace VolunteerHub.Controllers
{
    [Authorize(Roles = UserRoles.Organizer)]
    public class ShiftsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShiftsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // -------------------------------------------------
        // LIST SHIFTS
        // -------------------------------------------------

        public async Task<IActionResult> Index(int actionId)
        {
            var volunteerAction =
                await GetOwnedActionAsync(actionId);


            if (volunteerAction == null)
            {
                return NotFound();
            }


            var shifts =
                await _context.Shifts
                    .Where(s =>
                        s.VolunteerActionId == actionId)
                    .OrderBy(s =>
                        s.StartTime)
                    .ToListAsync();


            ViewBag.VolunteerAction =
                volunteerAction;


            return View(shifts);
        }


        // -------------------------------------------------
        // CREATE GET
        // -------------------------------------------------

        public async Task<IActionResult> Create(int actionId)
        {
            var volunteerAction =
                await GetOwnedActionAsync(actionId);


            if (volunteerAction == null)
            {
                return NotFound();
            }


            if (!CanManageShifts(
                volunteerAction.Status))
            {
                TempData["Error"] =
                    "Shifts can only be created while the opportunity is draft or published.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        actionId
                    });
            }


            var shift =
                new Shift
                {
                    VolunteerActionId =
                        actionId,

                    StartTime =
                        volunteerAction.StartDate,

                    EndTime =
                        volunteerAction.StartDate
                            .AddHours(2),

                    MaxVolunteers =
                        1
                };


            ViewBag.VolunteerAction =
                volunteerAction;


            return View(shift);
        }


        // -------------------------------------------------
        // CREATE POST
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Shift shift)
        {
            var volunteerAction =
                await GetOwnedActionAsync(
                    shift.VolunteerActionId);


            if (volunteerAction == null)
            {
                return NotFound();
            }


            if (!CanManageShifts(
                volunteerAction.Status))
            {
                TempData["Error"] =
                    "Shifts can only be created while the opportunity is draft or published.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        actionId =
                            shift.VolunteerActionId
                    });
            }


            ModelState.Remove(
                nameof(Shift.VolunteerAction));

            ModelState.Remove(
                nameof(Shift.ParticipationRequests));


            ValidateShift(
                shift,
                volunteerAction);


            if (!ModelState.IsValid)
            {
                ViewBag.VolunteerAction =
                    volunteerAction;

                return View(shift);
            }


            _context.Shifts.Add(shift);


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "The shift was created successfully.";


            return RedirectToAction(
                nameof(Index),
                new
                {
                    actionId =
                        shift.VolunteerActionId
                });
        }


        // -------------------------------------------------
        // EDIT GET
        // -------------------------------------------------

        public async Task<IActionResult> Edit(int id)
        {
            var shift =
                await _context.Shifts
                    .Include(s =>
                        s.VolunteerAction)
                    .FirstOrDefaultAsync(s =>
                        s.Id == id);


            if (shift == null)
            {
                return NotFound();
            }


            if (!IsOwner(
                shift.VolunteerAction))
            {
                return Forbid();
            }


            if (!CanManageShifts(
                shift.VolunteerAction.Status))
            {
                TempData["Error"] =
                    "Shifts can only be edited while the opportunity is draft or published.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        actionId =
                            shift.VolunteerActionId
                    });
            }


            await LoadEditViewDataAsync(
                shift);


            return View(shift);
        }


        // -------------------------------------------------
        // EDIT POST
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Shift model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }


            var shift =
                await _context.Shifts
                    .Include(s =>
                        s.VolunteerAction)
                    .FirstOrDefaultAsync(s =>
                        s.Id == id);


            if (shift == null)
            {
                return NotFound();
            }


            if (!IsOwner(
                shift.VolunteerAction))
            {
                return Forbid();
            }


            if (!CanManageShifts(
                shift.VolunteerAction.Status))
            {
                TempData["Error"] =
                    "Shifts can only be edited while the opportunity is draft or published.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        actionId =
                            shift.VolunteerActionId
                    });
            }


            ModelState.Remove(
                nameof(Shift.VolunteerAction));

            ModelState.Remove(
                nameof(Shift.ParticipationRequests));


            ValidateShift(
                model,
                shift.VolunteerAction);


            await ValidateEditRestrictionsAsync(
                shift,
                model);


            if (!ModelState.IsValid)
            {
                await LoadEditViewDataAsync(
                    shift);

                return View(model);
            }


            shift.Title =
                model.Title.Trim();

            shift.Description =
                model.Description.Trim();

            shift.StartTime =
                model.StartTime;

            shift.EndTime =
                model.EndTime;

            shift.MaxVolunteers =
                model.MaxVolunteers;

            shift.RequiredSkill =
                string.IsNullOrWhiteSpace(
                    model.RequiredSkill)
                    ? null
                    : model.RequiredSkill.Trim();

            shift.Notes =
                string.IsNullOrWhiteSpace(
                    model.Notes)
                    ? null
                    : model.Notes.Trim();


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "The shift was updated successfully.";


            return RedirectToAction(
                nameof(Index),
                new
                {
                    actionId =
                        shift.VolunteerActionId
                });
        }


        // -------------------------------------------------
        // DELETE
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var shift =
                await _context.Shifts
                    .Include(s =>
                        s.VolunteerAction)
                    .Include(s =>
                        s.ParticipationRequests)
                    .FirstOrDefaultAsync(s =>
                        s.Id == id);


            if (shift == null)
            {
                return NotFound();
            }


            if (!IsOwner(
                shift.VolunteerAction))
            {
                return Forbid();
            }


            if (!CanManageShifts(
                shift.VolunteerAction.Status))
            {
                TempData["Error"] =
                    "Shifts can only be deleted while the opportunity is draft or published.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        actionId =
                            shift.VolunteerActionId
                    });
            }


            if (shift.ParticipationRequests.Any())
            {
                TempData["Error"] =
                    "This shift cannot be deleted because it already has volunteer applications.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        actionId =
                            shift.VolunteerActionId
                    });
            }


            int actionId =
                shift.VolunteerActionId;


            _context.Shifts.Remove(shift);


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "The shift was deleted successfully.";


            return RedirectToAction(
                nameof(Index),
                new
                {
                    actionId
                });
        }


        // -------------------------------------------------
        // SHIFT VALIDATION
        // -------------------------------------------------

        private void ValidateShift(
            Shift shift,
            VolunteerAction volunteerAction)
        {
            if (shift.EndTime <=
                shift.StartTime)
            {
                ModelState.AddModelError(
                    nameof(Shift.EndTime),
                    "The end time must be after the start time.");
            }


            if (shift.StartTime <
                    volunteerAction.StartDate ||
                shift.StartTime >
                    volunteerAction.EndDate)
            {
                ModelState.AddModelError(
                    nameof(Shift.StartTime),
                    "The shift must start within the volunteer action schedule.");
            }


            if (shift.EndTime >
                    volunteerAction.EndDate ||
                shift.EndTime <
                    volunteerAction.StartDate)
            {
                ModelState.AddModelError(
                    nameof(Shift.EndTime),
                    "The shift must end within the volunteer action schedule.");
            }
        }


        // -------------------------------------------------
        // EDIT BUSINESS RULES
        // -------------------------------------------------

        private async Task ValidateEditRestrictionsAsync(
            Shift existingShift,
            Shift model)
        {
            bool hasApplications =
                await _context.ParticipationRequests
                    .AnyAsync(r =>
                        r.ShiftId ==
                        existingShift.Id);


            bool scheduleChanged =
                existingShift.StartTime !=
                    model.StartTime ||

                existingShift.EndTime !=
                    model.EndTime;


            // Once a published shift has applications,
            // its schedule must remain stable.
            if (existingShift.VolunteerAction.Status ==
                    VolunteerActionStatuses.Published &&
                hasApplications &&
                scheduleChanged)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The shift start and end times cannot be changed because volunteers have already applied.");
            }


            // Capacity must never become lower than
            // the number of already approved volunteers.
            int approvedCount =
                await _context.ParticipationRequests
                    .CountAsync(r =>
                        r.ShiftId ==
                            existingShift.Id &&

                        r.Status ==
                            ParticipationRequestStatuses.Approved);


            if (model.MaxVolunteers <
                approvedCount)
            {
                ModelState.AddModelError(
                    nameof(Shift.MaxVolunteers),
                    $"Capacity cannot be lower than the {approvedCount} already approved volunteer{(approvedCount == 1 ? "" : "s")}.");
            }
        }


        // -------------------------------------------------
        // EDIT VIEW INFORMATION
        // -------------------------------------------------

        private async Task LoadEditViewDataAsync(
            Shift shift)
        {
            ViewBag.VolunteerAction =
                shift.VolunteerAction;


            ViewBag.HasApplications =
                await _context.ParticipationRequests
                    .AnyAsync(r =>
                        r.ShiftId ==
                        shift.Id);


            ViewBag.ApprovedCount =
                await _context.ParticipationRequests
                    .CountAsync(r =>
                        r.ShiftId ==
                            shift.Id &&

                        r.Status ==
                            ParticipationRequestStatuses.Approved);
        }


        // -------------------------------------------------
        // STATUS CHECK
        // -------------------------------------------------

        private static bool CanManageShifts(
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

        private bool IsOwner(
            VolunteerAction volunteerAction)
        {
            var userId =
                _userManager.GetUserId(User);


            return volunteerAction.OrganizerId ==
                   userId;
        }


        private async Task<VolunteerAction?>
            GetOwnedActionAsync(int actionId)
        {
            var userId =
                _userManager.GetUserId(User);


            return await _context
                .VolunteerActions
                .FirstOrDefaultAsync(a =>
                    a.Id == actionId &&
                    a.OrganizerId ==
                    userId);
        }
    }
}