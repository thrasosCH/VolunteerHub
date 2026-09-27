using VolunteerHub.Models;

namespace VolunteerHub.ViewModels
{
    public class AdminDashboardViewModel
    {
        // USERS
        public int TotalUsers { get; set; }

        public int Volunteers { get; set; }

        public int Organizers { get; set; }

        public int Admins { get; set; }


        // OPPORTUNITIES
        public int TotalActions { get; set; }

        public int PublishedActions { get; set; }

        public int DraftActions { get; set; }

        public int CompletedActions { get; set; }


        // APPLICATIONS
        public int TotalApplications { get; set; }

        public int PendingApplications { get; set; }

        public int ApprovedApplications { get; set; }


        // PLATFORM
        public int TotalShifts { get; set; }

        public int Countries { get; set; }

        public int Cities { get; set; }


        // RECENT DATA
        public List<ApplicationUser> RecentUsers { get; set; }
            = new List<ApplicationUser>();

        public List<VolunteerAction> RecentActions { get; set; }
            = new List<VolunteerAction>();
    }
}