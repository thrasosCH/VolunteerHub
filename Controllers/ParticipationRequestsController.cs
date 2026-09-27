using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerHub.Constants;
using VolunteerHub.Data;
using VolunteerHub.Models;
using VolunteerHub.Services;

namespace VolunteerHub.Controllers
{
    [Authorize(Roles = UserRoles.Volunteer)]
    public class ParticipationRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITimeService _timeService;

        public ParticipationRequestsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ITimeService timeService)
        {
            _context = context;
            _userManager = userManager;
            _timeService = timeService;
        }


        // -------------------------------------------------
        // APPLY GET
        // -------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Apply(int shiftId)
        {
            var shift =
                await _context.Shifts
                    .Include(s => s.VolunteerAction)
                        .ThenInclude(a => a.City)
                            .ThenInclude(c => c!.Country)
                    .FirstOrDefaultAsync(s =>
                        s.Id == shiftId);


            if (shift == null)
            {
                return NotFound();
            }


            if (shift.VolunteerAction.Status !=
                VolunteerActionStatuses.Published)
            {
                TempData["Error"] =
                    "Applications are not currently open for this opportunity.";

                return RedirectToAction(
                    "Details",
                    "VolunteerActions",
                    new
                    {
                        id = shift.VolunteerActionId
                    });
            }


            if (!TryGetCurrentActionLocalTime(
                shift.VolunteerAction,
                out var now))
            {
                TempData["Error"] =
                    "The opportunity time zone could not be determined.";

                return RedirectToAction(
                    "Details",
                    "VolunteerActions",
                    new
                    {
                        id = shift.VolunteerActionId
                    });
            }


            if (now >
                shift.VolunteerAction.ApplicationDeadline)
            {
                TempData["Error"] =
                    "The application deadline for this opportunity has passed.";

                return RedirectToAction(
                    "Details",
                    "VolunteerActions",
                    new
                    {
                        id = shift.VolunteerActionId
                    });
            }


            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }


            bool alreadyApplied =
                await _context.ParticipationRequests
                    .AnyAsync(r =>
                        r.VolunteerId == userId &&
                        r.ShiftId == shiftId &&
                        (
                            r.Status ==
                                ParticipationRequestStatuses.Pending ||

                            r.Status ==
                                ParticipationRequestStatuses.Approved ||

                            r.Status ==
                                ParticipationRequestStatuses.Waitlist
                        ));


            if (alreadyApplied)
            {
                TempData["Error"] =
                    "You already have an active application for this shift.";

                return RedirectToAction(
                    "Details",
                    "VolunteerActions",
                    new
                    {
                        id = shift.VolunteerActionId
                    });
            }


            bool hasOverlap =
                await HasOverlappingApplicationAsync(
                    userId,
                    shift);


            if (hasOverlap)
            {
                TempData["Error"] =
                    "You already have an active application for another shift that overlaps with this time.";

                return RedirectToAction(
                    "Details",
                    "VolunteerActions",
                    new
                    {
                        id = shift.VolunteerActionId
                    });
            }


            ViewBag.Shift =
                shift;


            return View();
        }


        // -------------------------------------------------
        // APPLY POST
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(
            int shiftId,
            string? message)
        {
            var shift =
                await _context.Shifts
                    .Include(s => s.VolunteerAction)
                        .ThenInclude(a => a.City)
                            .ThenInclude(c => c!.Country)
                    .FirstOrDefaultAsync(s =>
                        s.Id == shiftId);


            if (shift == null)
            {
                return NotFound();
            }


            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }


            // -------------------------------------------------
            // ACTION MUST BE OPEN
            // -------------------------------------------------

            if (shift.VolunteerAction.Status !=
                VolunteerActionStatuses.Published)
            {
                TempData["Error"] =
                    "Applications are not currently open for this opportunity.";

                return RedirectToAction(
                    "Details",
                    "VolunteerActions",
                    new
                    {
                        id = shift.VolunteerActionId
                    });
            }


            // -------------------------------------------------
            // TIME ZONE
            // -------------------------------------------------

            if (!TryGetCurrentActionLocalTime(
                shift.VolunteerAction,
                out var now))
            {
                TempData["Error"] =
                    "The opportunity time zone could not be determined.";

                return RedirectToAction(
                    "Details",
                    "VolunteerActions",
                    new
                    {
                        id = shift.VolunteerActionId
                    });
            }


            // -------------------------------------------------
            // DEADLINE
            // -------------------------------------------------

            if (now >
                shift.VolunteerAction.ApplicationDeadline)
            {
                TempData["Error"] =
                    "The application deadline for this opportunity has passed.";

                return RedirectToAction(
                    "Details",
                    "VolunteerActions",
                    new
                    {
                        id = shift.VolunteerActionId
                    });
            }


            // -------------------------------------------------
            // DUPLICATE ACTIVE APPLICATION
            // -------------------------------------------------

            bool alreadyApplied =
                await _context.ParticipationRequests
                    .AnyAsync(r =>
                        r.VolunteerId == userId &&
                        r.ShiftId == shiftId &&
                        (
                            r.Status ==
                                ParticipationRequestStatuses.Pending ||

                            r.Status ==
                                ParticipationRequestStatuses.Approved ||

                            r.Status ==
                                ParticipationRequestStatuses.Waitlist
                        ));


            if (alreadyApplied)
            {
                TempData["Error"] =
                    "You already have an active application for this shift.";

                return RedirectToAction(
                    "Details",
                    "VolunteerActions",
                    new
                    {
                        id = shift.VolunteerActionId
                    });
            }


            // -------------------------------------------------
            // OVERLAPPING SHIFT CHECK
            // -------------------------------------------------

            bool hasOverlap =
                await HasOverlappingApplicationAsync(
                    userId,
                    shift);


            if (hasOverlap)
            {
                TempData["Error"] =
                    "You already have an active application for another shift that overlaps with this time.";

                return RedirectToAction(
                    "Details",
                    "VolunteerActions",
                    new
                    {
                        id = shift.VolunteerActionId
                    });
            }


            // -------------------------------------------------
            // MESSAGE VALIDATION
            // -------------------------------------------------

            if (!string.IsNullOrWhiteSpace(message) &&
                message.Length > 1000)
            {
                ModelState.AddModelError(
                    "Message",
                    "Your message cannot exceed 1000 characters.");


                ViewBag.Shift =
                    shift;


                return View();
            }


            // -------------------------------------------------
            // CAPACITY CHECK
            // -------------------------------------------------

            int approvedCount =
                await _context.ParticipationRequests
                    .CountAsync(r =>
                        r.ShiftId == shiftId &&
                        r.Status ==
                            ParticipationRequestStatuses.Approved);


            string status =
                approvedCount >= shift.MaxVolunteers
                    ? ParticipationRequestStatuses.Waitlist
                    : ParticipationRequestStatuses.Pending;


            var request =
                new ParticipationRequest
                {
                    VolunteerId =
                        userId,

                    ShiftId =
                        shiftId,

                    Message =
                        string.IsNullOrWhiteSpace(message)
                            ? null
                            : message.Trim(),

                    ApplicationDate =
                        _timeService.UtcNow,

                    Status =
                        status,

                    Attended =
                        false
                };


            _context.ParticipationRequests.Add(
                request);


            await _context.SaveChangesAsync();


            if (status ==
                ParticipationRequestStatuses.Waitlist)
            {
                TempData["Success"] =
                    "The shift is currently full. Your application has been added to the waitlist.";
            }
            else
            {
                TempData["Success"] =
                    "Your application was submitted successfully.";
            }


            return RedirectToAction(
                nameof(MyRequests));
        }


        // -------------------------------------------------
        // MY APPLICATIONS
        // -------------------------------------------------

        public async Task<IActionResult> MyRequests()
        {
            var userId =
                _userManager.GetUserId(User);


            var requests =
                await _context.ParticipationRequests
                    .Include(r => r.Shift)
                        .ThenInclude(s => s.VolunteerAction)
                            .ThenInclude(a => a.City)
                                .ThenInclude(c => c!.Country)
                    .Where(r =>
                        r.VolunteerId == userId)
                    .OrderByDescending(r =>
                        r.ApplicationDate)
                    .ToListAsync();


            return View(requests);
        }


        // -------------------------------------------------
        // CANCEL APPLICATION
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId =
                _userManager.GetUserId(User);


            var request =
                await _context.ParticipationRequests
                    .Include(r => r.Shift)
                        .ThenInclude(s => s.VolunteerAction)
                            .ThenInclude(a => a.City)
                                .ThenInclude(c => c!.Country)
                    .FirstOrDefaultAsync(r =>
                        r.Id == id &&
                        r.VolunteerId == userId);


            if (request == null)
            {
                return NotFound();
            }


            bool validApplicationStatus =
                request.Status ==
                    ParticipationRequestStatuses.Pending ||

                request.Status ==
                    ParticipationRequestStatuses.Approved ||

                request.Status ==
                    ParticipationRequestStatuses.Waitlist;


            if (!validApplicationStatus)
            {
                TempData["Error"] =
                    "This application can no longer be cancelled.";

                return RedirectToAction(
                    nameof(MyRequests));
            }


            bool validActionStatus =
                request.Shift.VolunteerAction.Status ==
                    VolunteerActionStatuses.Published ||

                request.Shift.VolunteerAction.Status ==
                    VolunteerActionStatuses.InProgress;


            if (!validActionStatus)
            {
                TempData["Error"] =
                    "This application cannot be cancelled because the opportunity is no longer active.";

                return RedirectToAction(
                    nameof(MyRequests));
            }


            if (!TryGetCurrentActionLocalTime(
                request.Shift.VolunteerAction,
                out var now))
            {
                TempData["Error"] =
                    "The opportunity time zone could not be determined.";

                return RedirectToAction(
                    nameof(MyRequests));
            }


            if (now >=
                request.Shift.StartTime)
            {
                TempData["Error"] =
                    "This application cannot be cancelled because the shift has already started.";

                return RedirectToAction(
                    nameof(MyRequests));
            }


            bool wasApproved =
                request.Status ==
                    ParticipationRequestStatuses.Approved;


            request.Status =
                ParticipationRequestStatuses.Cancelled;


            // -------------------------------------------------
            // WAITLIST PROMOTION
            // -------------------------------------------------

            if (wasApproved)
            {
                var nextWaitlistedRequest =
                    await _context.ParticipationRequests
                        .Where(r =>
                            r.ShiftId ==
                                request.ShiftId &&

                            r.Status ==
                                ParticipationRequestStatuses.Waitlist &&

                            r.Id !=
                                request.Id)
                        .OrderBy(r =>
                            r.ApplicationDate)
                        .FirstOrDefaultAsync();


                if (nextWaitlistedRequest != null)
                {
                    nextWaitlistedRequest.Status =
                        ParticipationRequestStatuses.Pending;
                }
            }


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Your application was cancelled successfully.";


            return RedirectToAction(
                nameof(MyRequests));
        }


        // -------------------------------------------------
        // OVERLAPPING APPLICATION CHECK
        // -------------------------------------------------

        private async Task<bool>
            HasOverlappingApplicationAsync(
                string volunteerId,
                Shift selectedShift)
        {
            return await _context.ParticipationRequests
                .AnyAsync(r =>
                    r.VolunteerId ==
                        volunteerId &&

                    r.ShiftId !=
                        selectedShift.Id &&

                    (
                        r.Status ==
                            ParticipationRequestStatuses.Pending ||

                        r.Status ==
                            ParticipationRequestStatuses.Approved ||

                        r.Status ==
                            ParticipationRequestStatuses.Waitlist
                    ) &&

                    r.Shift.StartTime <
                        selectedShift.EndTime &&

                    r.Shift.EndTime >
                        selectedShift.StartTime);
        }


        // -------------------------------------------------
        // ACTION LOCAL TIME
        // -------------------------------------------------

        private bool TryGetCurrentActionLocalTime(
            VolunteerAction volunteerAction,
            out DateTime localNow)
        {
            localNow =
                default;


            var timeZoneId =
                volunteerAction.City?
                    .Country?
                    .TimeZoneId;


            if (string.IsNullOrWhiteSpace(timeZoneId))
            {
                return false;
            }


            localNow =
                _timeService.GetCurrentLocalTime(
                    timeZoneId);


            return true;
        }
    }
}