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
    [Authorize(Roles = UserRoles.Organizer)]
    public class OrganizerRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITimeService _timeService;

        public OrganizerRequestsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ITimeService timeService)
        {
            _context = context;
            _userManager = userManager;
            _timeService = timeService;
        }


        // -------------------------------------------------
        // ALL APPLICATIONS FOR CURRENT ORGANIZER
        // -------------------------------------------------

        public async Task<IActionResult> Index()
        {
            var organizerId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(organizerId))
            {
                return Unauthorized();
            }


            var requests =
                await _context.ParticipationRequests
                    .Include(r => r.Volunteer)
                    .Include(r => r.Shift)
                        .ThenInclude(s => s.VolunteerAction)
                            .ThenInclude(a => a.City)
                                .ThenInclude(c => c!.Country)
                    .Where(r =>
                        r.Shift.VolunteerAction.OrganizerId ==
                        organizerId)
                    .OrderBy(r =>
                        r.Status ==
                            ParticipationRequestStatuses.Pending
                            ? 0
                            : r.Status ==
                                ParticipationRequestStatuses.Waitlist
                                ? 1
                                : r.Status ==
                                    ParticipationRequestStatuses.Approved
                                    ? 2
                                    : 3)
                    .ThenByDescending(r =>
                        r.ApplicationDate)
                    .ToListAsync();


            return View(requests);
        }


        // -------------------------------------------------
        // APPROVE APPLICATION
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var request =
                await GetOwnedRequestAsync(id);


            if (request == null)
            {
                return NotFound();
            }


            if (request.Status !=
                    ParticipationRequestStatuses.Pending &&
                request.Status !=
                    ParticipationRequestStatuses.Waitlist)
            {
                TempData["Error"] =
                    "Only pending or waitlisted applications can be approved.";

                return RedirectToAction(
                    nameof(Index));
            }


            // -------------------------------------------------
            // CURRENT LOCAL TIME
            // -------------------------------------------------

            if (!TryGetCurrentActionLocalTime(
                request.Shift.VolunteerAction,
                out var now))
            {
                TempData["Error"] =
                    "The opportunity time zone could not be determined.";

                return RedirectToAction(
                    nameof(Index));
            }


            // -------------------------------------------------
            // ACTION / SHIFT MUST STILL BE OPEN FOR REVIEW
            // -------------------------------------------------

            if (!CanReviewApplication(
                request,
                now))
            {
                TempData["Error"] =
                    "Applications can only be approved while the opportunity is published and before the shift starts.";

                return RedirectToAction(
                    nameof(Index));
            }


            // -------------------------------------------------
            // CHECK SHIFT CAPACITY
            // -------------------------------------------------

            int approvedCount =
                await _context.ParticipationRequests
                    .CountAsync(r =>
                        r.ShiftId ==
                            request.ShiftId &&

                        r.Status ==
                            ParticipationRequestStatuses.Approved &&

                        r.Id !=
                            request.Id);


            if (approvedCount >=
                request.Shift.MaxVolunteers)
            {
                TempData["Error"] =
                    "This shift has reached its maximum volunteer capacity.";

                return RedirectToAction(
                    nameof(Index));
            }


            // -------------------------------------------------
            // CHECK FOR APPROVED OVERLAPPING SHIFT
            // -------------------------------------------------

            bool hasConflict =
                await _context.ParticipationRequests
                    .AnyAsync(r =>
                        r.VolunteerId ==
                            request.VolunteerId &&

                        r.Id !=
                            request.Id &&

                        r.Status ==
                            ParticipationRequestStatuses.Approved &&

                        r.Shift.StartTime <
                            request.Shift.EndTime &&

                        r.Shift.EndTime >
                            request.Shift.StartTime);


            if (hasConflict)
            {
                TempData["Error"] =
                    "This volunteer already has another approved shift that overlaps with this time.";

                return RedirectToAction(
                    nameof(Index));
            }


            request.Status =
                ParticipationRequestStatuses.Approved;


            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"{request.Volunteer.FullName}'s application was approved.";


            return RedirectToAction(
                nameof(Index));
        }


        // -------------------------------------------------
        // REJECT APPLICATION
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var request =
                await GetOwnedRequestAsync(id);


            if (request == null)
            {
                return NotFound();
            }


            if (request.Status !=
                    ParticipationRequestStatuses.Pending &&
                request.Status !=
                    ParticipationRequestStatuses.Waitlist)
            {
                TempData["Error"] =
                    "Only pending or waitlisted applications can be rejected.";

                return RedirectToAction(
                    nameof(Index));
            }


            // -------------------------------------------------
            // CURRENT LOCAL TIME
            // -------------------------------------------------

            if (!TryGetCurrentActionLocalTime(
                request.Shift.VolunteerAction,
                out var now))
            {
                TempData["Error"] =
                    "The opportunity time zone could not be determined.";

                return RedirectToAction(
                    nameof(Index));
            }


            // -------------------------------------------------
            // ACTION / SHIFT MUST STILL BE OPEN FOR REVIEW
            // -------------------------------------------------

            if (!CanReviewApplication(
                request,
                now))
            {
                TempData["Error"] =
                    "Applications can only be rejected while the opportunity is published and before the shift starts.";

                return RedirectToAction(
                    nameof(Index));
            }


            request.Status =
                ParticipationRequestStatuses.Rejected;


            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"{request.Volunteer.FullName}'s application was rejected.";


            return RedirectToAction(
                nameof(Index));
        }


        // -------------------------------------------------
        // MARK VOLUNTEER AS ATTENDED
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttended(int id)
        {
            var request =
                await GetOwnedRequestAsync(id);


            if (request == null)
            {
                return NotFound();
            }


            if (request.Status !=
                ParticipationRequestStatuses.Approved)
            {
                TempData["Error"] =
                    "Only approved applications can be marked as attended.";

                return RedirectToAction(
                    nameof(Index));
            }


            // -------------------------------------------------
            // CURRENT LOCAL TIME
            // -------------------------------------------------

            if (!TryGetCurrentActionLocalTime(
                request.Shift.VolunteerAction,
                out var now))
            {
                TempData["Error"] =
                    "The opportunity time zone could not be determined.";

                return RedirectToAction(
                    nameof(Index));
            }


            // -------------------------------------------------
            // ATTENDANCE TIMING / ACTION STATUS CHECK
            // -------------------------------------------------

            if (!CanRecordAttendance(
                request,
                now))
            {
                TempData["Error"] =
                    "Attendance can only be recorded after the shift has started and while the opportunity is in progress or completed.";

                return RedirectToAction(
                    nameof(Index));
            }


            request.Attended =
                true;


            request.Status =
                ParticipationRequestStatuses.Completed;


            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"{request.Volunteer.FullName} was marked as attended.";


            return RedirectToAction(
                nameof(Index));
        }


        // -------------------------------------------------
        // APPLICATION REVIEW RULE
        // -------------------------------------------------

        private static bool CanReviewApplication(
            ParticipationRequest request,
            DateTime now)
        {
            return
                request.Shift.VolunteerAction.Status ==
                    VolunteerActionStatuses.Published &&

                now <
                    request.Shift.StartTime;
        }


        // -------------------------------------------------
        // ATTENDANCE RULE
        // -------------------------------------------------

        private static bool CanRecordAttendance(
            ParticipationRequest request,
            DateTime now)
        {
            bool validActionStatus =
                request.Shift.VolunteerAction.Status ==
                    VolunteerActionStatuses.InProgress ||

                request.Shift.VolunteerAction.Status ==
                    VolunteerActionStatuses.Completed;


            bool shiftHasStarted =
                now >=
                    request.Shift.StartTime;


            return
                validActionStatus &&
                shiftHasStarted;
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


        // -------------------------------------------------
        // OWNERSHIP + FULL REQUEST DATA
        // -------------------------------------------------

        private async Task<ParticipationRequest?>
            GetOwnedRequestAsync(int id)
        {
            var organizerId =
                _userManager.GetUserId(User);


            return await _context.ParticipationRequests
                .Include(r => r.Volunteer)
                .Include(r => r.Shift)
                    .ThenInclude(s =>
                        s.VolunteerAction)
                        .ThenInclude(a =>
                            a.City)
                            .ThenInclude(c =>
                                c!.Country)
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&

                    r.Shift.VolunteerAction.OrganizerId ==
                        organizerId);
        }
    }
}