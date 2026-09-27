using VolunteerHub.Models;

namespace VolunteerHub.ViewModels
{
    public class VolunteerDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;

        public int TotalApplications { get; set; }

        public int PendingApplications { get; set; }

        public int ApprovedApplications { get; set; }

        public int WaitlistApplications { get; set; }

        public int CompletedApplications { get; set; }

        public int AttendedActivities { get; set; }

        public List<ParticipationRequest> UpcomingApplications { get; set; }
            = new List<ParticipationRequest>();

        public List<ParticipationRequest> RecentApplications { get; set; }
            = new List<ParticipationRequest>();
    }
}