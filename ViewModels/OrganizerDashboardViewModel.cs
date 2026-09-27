using VolunteerHub.Models;

namespace VolunteerHub.ViewModels
{
    public class OrganizerDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;

        public int TotalActions { get; set; }

        public int ActiveActions { get; set; }

        public int PendingApplications { get; set; }

        public int ApprovedVolunteers { get; set; }

        public int StartingSoon { get; set; }

        public int AvailableShifts { get; set; }

        public List<ParticipationRequest> RecentApplications { get; set; }
            = new List<ParticipationRequest>();

        public List<VolunteerAction> UpcomingActions { get; set; }
            = new List<VolunteerAction>();
    }
}