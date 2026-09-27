using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerHub.Constants;
using VolunteerHub.Data;
using VolunteerHub.Models;
using VolunteerHub.ViewModels;

namespace VolunteerHub.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // -------------------------------------------------
        // ADMIN DASHBOARD
        // -------------------------------------------------

        public async Task<IActionResult> Index()
        {
            var volunteers =
                await _userManager.GetUsersInRoleAsync(UserRoles.Volunteer);

            var organizers =
                await _userManager.GetUsersInRoleAsync(UserRoles.Organizer);

            var admins =
                await _userManager.GetUsersInRoleAsync(UserRoles.Admin);


            var model =
                new AdminDashboardViewModel
                {
                    // -------------------------------------------------
                    // USERS
                    // -------------------------------------------------

                    TotalUsers =
                        await _context.Users.CountAsync(),

                    Volunteers =
                        volunteers.Count,

                    Organizers =
                        organizers.Count,

                    Admins =
                        admins.Count,


                    // -------------------------------------------------
                    // OPPORTUNITIES
                    // -------------------------------------------------

                    TotalActions =
                        await _context.VolunteerActions
                            .CountAsync(),

                    PublishedActions =
                        await _context.VolunteerActions
                            .CountAsync(a =>
                                a.Status ==
                                VolunteerActionStatuses.Published),

                    DraftActions =
                        await _context.VolunteerActions
                            .CountAsync(a =>
                                a.Status ==
                                VolunteerActionStatuses.Draft),

                    CompletedActions =
                        await _context.VolunteerActions
                            .CountAsync(a =>
                                a.Status ==
                                VolunteerActionStatuses.Completed),


                    // -------------------------------------------------
                    // APPLICATIONS
                    // -------------------------------------------------

                    TotalApplications =
                        await _context.ParticipationRequests
                            .CountAsync(),

                    PendingApplications =
                        await _context.ParticipationRequests
                            .CountAsync(r =>
                                r.Status ==
                                ParticipationRequestStatuses.Pending),

                    ApprovedApplications =
                        await _context.ParticipationRequests
                            .CountAsync(r =>
                                r.Status ==
                                ParticipationRequestStatuses.Approved),


                    // -------------------------------------------------
                    // PLATFORM
                    // -------------------------------------------------

                    TotalShifts =
                        await _context.Shifts.CountAsync(),

                    Countries =
                        await _context.Countries.CountAsync(),

                    Cities =
                        await _context.Cities.CountAsync(),


                    // -------------------------------------------------
                    // RECENT USERS
                    // -------------------------------------------------

                    RecentUsers =
                        await _context.Users
                            .Include(u => u.City)
                                .ThenInclude(c => c!.Country)
                            .OrderByDescending(u =>
                                u.RegistrationDate)
                            .Take(5)
                            .ToListAsync(),


                    // -------------------------------------------------
                    // RECENT OPPORTUNITIES
                    // -------------------------------------------------

                    RecentActions =
                        await _context.VolunteerActions
                            .Include(a => a.City)
                                .ThenInclude(c => c!.Country)
                            .Include(a => a.Organizer)
                            .OrderByDescending(a => a.Id)
                            .Take(5)
                            .ToListAsync()
                };


            return View(model);
        }


        // -------------------------------------------------
        // USERS
        // -------------------------------------------------

        public async Task<IActionResult> Users()
        {
            var users =
                await _context.Users
                    .Include(u => u.City)
                        .ThenInclude(c => c!.Country)
                    .OrderByDescending(u =>
                        u.RegistrationDate)
                    .ToListAsync();


            var model =
                new List<AdminUserViewModel>();


            foreach (var user in users)
            {
                var roles =
                    await _userManager.GetRolesAsync(user);


                model.Add(
                    new AdminUserViewModel
                    {
                        Id =
                            user.Id,

                        FullName =
                            user.FullName,

                        Email =
                            user.Email ?? string.Empty,

                        Role =
                            roles.FirstOrDefault()
                            ?? "No Role",

                        Location =
                            user.City != null &&
                            user.City.Country != null
                                ? $"{user.City.Name}, {user.City.Country.Name}"
                                : "Not specified",

                        RegistrationDate =
                            user.RegistrationDate
                    });
            }


            return View(model);
        }


        // -------------------------------------------------
        // DELETE USER
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(
            string id)
        {
            var user =
                await _userManager.FindByIdAsync(id);


            if (user == null)
            {
                return NotFound();
            }


            // -------------------------------------------------
            // PROTECT ADMIN ACCOUNTS
            // -------------------------------------------------

            if (await _userManager.IsInRoleAsync(
                    user,
                    UserRoles.Admin))
            {
                TempData["Error"] =
                    "Admin accounts cannot be deleted.";

                return RedirectToAction(
                    nameof(Users));
            }


            // -------------------------------------------------
            // DELETE VOLUNTEER APPLICATIONS
            // -------------------------------------------------

            var volunteerRequests =
                await _context.ParticipationRequests
                    .Where(r =>
                        r.VolunteerId == user.Id)
                    .ToListAsync();


            if (volunteerRequests.Any())
            {
                _context.ParticipationRequests
                    .RemoveRange(volunteerRequests);
            }


            // -------------------------------------------------
            // DELETE ORGANIZER ACTIONS
            // Shifts and their participation requests
            // are deleted through cascade relationships.
            // -------------------------------------------------

            var organizerActions =
                await _context.VolunteerActions
                    .Where(a =>
                        a.OrganizerId == user.Id)
                    .ToListAsync();


            if (organizerActions.Any())
            {
                _context.VolunteerActions
                    .RemoveRange(organizerActions);
            }


            await _context.SaveChangesAsync();


            // -------------------------------------------------
            // DELETE IDENTITY USER
            // -------------------------------------------------

            var result =
                await _userManager.DeleteAsync(user);


            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    TempData["Error"] =
                        error.Description;

                    break;
                }


                return RedirectToAction(
                    nameof(Users));
            }


            TempData["Success"] =
                $"User {user.Email} was deleted successfully.";


            return RedirectToAction(
                nameof(Users));
        }


        // -------------------------------------------------
        // OPPORTUNITIES
        // -------------------------------------------------

        public async Task<IActionResult> Actions()
        {
            var actions =
                await _context.VolunteerActions
                    .Include(a => a.Organizer)
                    .Include(a => a.City)
                        .ThenInclude(c => c!.Country)
                    .Include(a => a.Shifts)
                    .OrderByDescending(a => a.Id)
                    .ToListAsync();


            return View(actions);
        }


        // -------------------------------------------------
        // DELETE OPPORTUNITY
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAction(
            int id)
        {
            var volunteerAction =
                await _context.VolunteerActions
                    .Include(a => a.Shifts)
                        .ThenInclude(s =>
                            s.ParticipationRequests)
                    .FirstOrDefaultAsync(a =>
                        a.Id == id);


            if (volunteerAction == null)
            {
                return NotFound();
            }


            _context.VolunteerActions
                .Remove(volunteerAction);


            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"Opportunity \"{volunteerAction.Title}\" was deleted successfully.";


            return RedirectToAction(
                nameof(Actions));
        }
    }
}