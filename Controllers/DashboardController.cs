using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerHub.Constants;
using VolunteerHub.Data;
using VolunteerHub.Models;
using VolunteerHub.Services;
using VolunteerHub.ViewModels;

namespace VolunteerHub.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITimeService _timeService;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ITimeService timeService)
        {
            _context = context;
            _userManager = userManager;
            _timeService = timeService;
        }


        // -------------------------------------------------
        // DASHBOARD ROUTER
        // -------------------------------------------------

        public IActionResult Index()
        {
            if (User.IsInRole(
                UserRoles.Organizer))
            {
                return RedirectToAction(
                    nameof(Organizer));
            }


            if (User.IsInRole(
                UserRoles.Volunteer))
            {
                return RedirectToAction(
                    nameof(Volunteer));
            }


            return RedirectToAction(
                "Index",
                "Home");
        }


        // -------------------------------------------------
        // VOLUNTEER DASHBOARD
        // -------------------------------------------------

        [Authorize(Roles = UserRoles.Volunteer)]
        public async Task<IActionResult> Volunteer()
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }


            var user =
                await _userManager.GetUserAsync(User);


            if (user == null)
            {
                return NotFound();
            }


            var requests =
                await _context.ParticipationRequests
                    .Include(r => r.Shift)
                        .ThenInclude(s => s.VolunteerAction)
                            .ThenInclude(a => a.City)
                                .ThenInclude(c => c!.Country)
                    .Where(r =>
                        r.VolunteerId == userId)
                    .ToListAsync();


            var model =
                new VolunteerDashboardViewModel
                {
                    FullName =
                        user.FullName,


                    TotalApplications =
                        requests.Count,


                    PendingApplications =
                        requests.Count(r =>
                            r.Status ==
                            ParticipationRequestStatuses.Pending),


                    ApprovedApplications =
                        requests.Count(r =>
                            r.Status ==
                            ParticipationRequestStatuses.Approved),


                    WaitlistApplications =
                        requests.Count(r =>
                            r.Status ==
                            ParticipationRequestStatuses.Waitlist),


                    CompletedApplications =
                        requests.Count(r =>
                            r.Status ==
                            ParticipationRequestStatuses.Completed),


                    AttendedActivities =
                        requests.Count(r =>
                            r.Status ==
                                ParticipationRequestStatuses.Completed &&
                            r.Attended),


                    UpcomingApplications =
                        requests
                            .Where(r =>
                                r.Status ==
                                    ParticipationRequestStatuses.Approved &&
                                IsShiftUpcoming(r))
                            .OrderBy(r =>
                                r.Shift.StartTime)
                            .Take(5)
                            .ToList(),


                    RecentApplications =
                        requests
                            .OrderByDescending(r =>
                                r.ApplicationDate)
                            .Take(5)
                            .ToList()
                };


            return View(model);
        }


        // -------------------------------------------------
        // ORGANIZER DASHBOARD
        // -------------------------------------------------

        [Authorize(Roles = UserRoles.Organizer)]
        public async Task<IActionResult> Organizer()
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }


            var user =
                await _userManager.GetUserAsync(User);


            if (user == null)
            {
                return NotFound();
            }


            // -------------------------------------------------
            // ORGANIZER ACTIONS
            // -------------------------------------------------

            var organizerActions =
                await _context.VolunteerActions
                    .Include(a => a.City)
                        .ThenInclude(c => c!.Country)
                    .Include(a => a.Shifts)
                        .ThenInclude(s => s.ParticipationRequests)
                    .Where(a =>
                        a.OrganizerId == userId)
                    .ToListAsync();


            // -------------------------------------------------
            // RECENT APPLICATIONS
            // -------------------------------------------------

            var recentApplications =
                await _context.ParticipationRequests
                    .Include(r => r.Volunteer)
                    .Include(r => r.Shift)
                        .ThenInclude(s => s.VolunteerAction)
                            .ThenInclude(a => a.City)
                                .ThenInclude(c => c!.Country)
                    .Where(r =>
                        r.Shift.VolunteerAction.OrganizerId ==
                        userId)
                    .OrderByDescending(r =>
                        r.ApplicationDate)
                    .Take(5)
                    .ToListAsync();


            // -------------------------------------------------
            // PENDING APPLICATIONS
            // -------------------------------------------------

            int pendingApplications =
                await _context.ParticipationRequests
                    .CountAsync(r =>
                        r.Shift.VolunteerAction.OrganizerId ==
                            userId &&
                        r.Status ==
                            ParticipationRequestStatuses.Pending);


            // -------------------------------------------------
            // UNIQUE APPROVED VOLUNTEERS
            // -------------------------------------------------

            int approvedVolunteers =
                await _context.ParticipationRequests
                    .Where(r =>
                        r.Shift.VolunteerAction.OrganizerId ==
                            userId &&
                        r.Status ==
                            ParticipationRequestStatuses.Approved)
                    .Select(r =>
                        r.VolunteerId)
                    .Distinct()
                    .CountAsync();


            // -------------------------------------------------
            // SHIFTS WITH AVAILABLE CAPACITY
            // -------------------------------------------------

            int availableShifts =
                organizerActions
                    .SelectMany(a =>
                        a.Shifts.Select(s =>
                            new
                            {
                                Action = a,
                                Shift = s
                            }))
                    .Count(x =>
                        (
                            x.Action.Status ==
                                VolunteerActionStatuses.Published ||

                            x.Action.Status ==
                                VolunteerActionStatuses.InProgress
                        ) &&
                        IsShiftUpcoming(
                            x.Shift,
                            x.Action) &&
                        x.Shift.ParticipationRequests.Count(r =>
                            r.Status ==
                                ParticipationRequestStatuses.Approved)
                        < x.Shift.MaxVolunteers);


            var model =
                new OrganizerDashboardViewModel
                {
                    FullName =
                        user.FullName,


                    TotalActions =
                        organizerActions.Count,


                    ActiveActions =
                        organizerActions.Count(a =>
                            a.Status ==
                                VolunteerActionStatuses.Published ||

                            a.Status ==
                                VolunteerActionStatuses.InProgress),


                    PendingApplications =
                        pendingApplications,


                    ApprovedVolunteers =
                        approvedVolunteers,


                    StartingSoon =
                        organizerActions.Count(a =>
                            a.Status ==
                                VolunteerActionStatuses.Published &&
                            IsStartingSoon(a)),


                    AvailableShifts =
                        availableShifts,


                    UpcomingActions =
                        organizerActions
                            .Where(a =>
                                (
                                    a.Status ==
                                        VolunteerActionStatuses.Published ||

                                    a.Status ==
                                        VolunteerActionStatuses.InProgress
                                ) &&
                                HasNotEnded(a))
                            .OrderBy(a =>
                                a.StartDate)
                            .Take(5)
                            .ToList(),


                    RecentApplications =
                        recentApplications
                };


            return View(model);
        }


        // -------------------------------------------------
        // ORGANIZER STATISTICS
        // -------------------------------------------------

        [Authorize(Roles = UserRoles.Organizer)]
        public async Task<IActionResult> OrganizerStatistics()
        {
            var organizerId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(organizerId))
            {
                return Unauthorized();
            }


            var actions =
                await _context.VolunteerActions
                    .Include(a => a.City)
                        .ThenInclude(c => c!.Country)
                    .Include(a => a.Shifts)
                        .ThenInclude(s => s.ParticipationRequests)
                    .Where(a =>
                        a.OrganizerId == organizerId)
                    .OrderByDescending(a =>
                        a.StartDate)
                    .ToListAsync();


            var allRequests =
                actions
                    .SelectMany(a => a.Shifts)
                    .SelectMany(s =>
                        s.ParticipationRequests)
                    .ToList();


            var model =
                new OrganizerStatisticsViewModel
                {
                    // -------------------------------------------------
                    // ACTIONS
                    // -------------------------------------------------

                    TotalActions =
                        actions.Count,


                    PublishedActions =
                        actions.Count(a =>
                            a.Status ==
                            VolunteerActionStatuses.Published),


                    InProgressActions =
                        actions.Count(a =>
                            a.Status ==
                            VolunteerActionStatuses.InProgress),


                    CompletedActions =
                        actions.Count(a =>
                            a.Status ==
                            VolunteerActionStatuses.Completed),


                    CancelledActions =
                        actions.Count(a =>
                            a.Status ==
                            VolunteerActionStatuses.Cancelled),


                    // -------------------------------------------------
                    // APPLICATIONS
                    // -------------------------------------------------

                    TotalApplications =
                        allRequests.Count,


                    PendingApplications =
                        allRequests.Count(r =>
                            r.Status ==
                            ParticipationRequestStatuses.Pending),


                    ApprovedApplications =
                        allRequests.Count(r =>
                            r.Status ==
                            ParticipationRequestStatuses.Approved),


                    WaitlistApplications =
                        allRequests.Count(r =>
                            r.Status ==
                            ParticipationRequestStatuses.Waitlist),


                    RejectedApplications =
                        allRequests.Count(r =>
                            r.Status ==
                            ParticipationRequestStatuses.Rejected),


                    CompletedApplications =
                        allRequests.Count(r =>
                            r.Status ==
                            ParticipationRequestStatuses.Completed),


                    // -------------------------------------------------
                    // COMMUNITY
                    // -------------------------------------------------

                    UniqueVolunteers =
                        allRequests
                            .Select(r =>
                                r.VolunteerId)
                            .Distinct()
                            .Count(),


                    TotalShifts =
                        actions.Sum(a =>
                            a.Shifts.Count),


                    TotalCapacity =
                        actions
                            .SelectMany(a =>
                                a.Shifts)
                            .Sum(s =>
                                s.MaxVolunteers),


                    ApprovedPlaces =
                        allRequests.Count(r =>
                            r.Status ==
                                ParticipationRequestStatuses.Approved ||

                            r.Status ==
                                ParticipationRequestStatuses.Completed),


                    // -------------------------------------------------
                    // PER ACTION
                    // -------------------------------------------------

                    Actions =
                        actions
                            .Select(a =>
                            {
                                var requests =
                                    a.Shifts
                                        .SelectMany(s =>
                                            s.ParticipationRequests)
                                        .ToList();


                                return new OrganizerActionStatisticsViewModel
                                {
                                    ActionId =
                                        a.Id,


                                    Title =
                                        a.Title,


                                    Status =
                                        a.Status,


                                    Category =
                                        a.Category,


                                    Location =
                                        a.City != null &&
                                        a.City.Country != null
                                            ? $"{a.City.Name}, {a.City.Country.Name}"
                                            : "Not specified",


                                    StartDate =
                                        a.StartDate,


                                    Shifts =
                                        a.Shifts.Count,


                                    Applications =
                                        requests.Count,


                                    Approved =
                                        requests.Count(r =>
                                            r.Status ==
                                                ParticipationRequestStatuses.Approved),


                                    Pending =
                                        requests.Count(r =>
                                            r.Status ==
                                                ParticipationRequestStatuses.Pending),


                                    Completed =
                                        requests.Count(r =>
                                            r.Status ==
                                                ParticipationRequestStatuses.Completed)
                                };
                            })
                            .ToList()
                };


            return View(model);
        }


        // -------------------------------------------------
        // VOLUNTEER UPCOMING SHIFT
        // -------------------------------------------------

        private bool IsShiftUpcoming(
            ParticipationRequest request)
        {
            return IsShiftUpcoming(
                request.Shift,
                request.Shift.VolunteerAction);
        }


        // -------------------------------------------------
        // SHIFT UPCOMING CHECK
        // -------------------------------------------------

        private bool IsShiftUpcoming(
            Shift shift,
            VolunteerAction volunteerAction)
        {
            if (!TryGetCurrentActionLocalTime(
                volunteerAction,
                out var now))
            {
                return false;
            }


            return
                shift.StartTime >= now;
        }


        // -------------------------------------------------
        // STARTING WITHIN NEXT 7 DAYS
        // -------------------------------------------------

        private bool IsStartingSoon(
            VolunteerAction volunteerAction)
        {
            if (!TryGetCurrentActionLocalTime(
                volunteerAction,
                out var now))
            {
                return false;
            }


            var nextWeek =
                now.AddDays(7);


            return
                volunteerAction.StartDate >= now &&
                volunteerAction.StartDate <= nextWeek;
        }


        // -------------------------------------------------
        // ACTION HAS NOT ENDED
        // -------------------------------------------------

        private bool HasNotEnded(
            VolunteerAction volunteerAction)
        {
            if (!TryGetCurrentActionLocalTime(
                volunteerAction,
                out var now))
            {
                return false;
            }


            return
                volunteerAction.EndDate >= now;
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