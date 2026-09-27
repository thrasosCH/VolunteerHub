namespace VolunteerHub.ViewModels
{
    public class OrganizerStatisticsViewModel
    {
        // ACTIONS
        public int TotalActions { get; set; }

        public int PublishedActions { get; set; }

        public int InProgressActions { get; set; }

        public int CompletedActions { get; set; }

        public int CancelledActions { get; set; }


        // APPLICATIONS
        public int TotalApplications { get; set; }

        public int PendingApplications { get; set; }

        public int ApprovedApplications { get; set; }

        public int WaitlistApplications { get; set; }

        public int RejectedApplications { get; set; }

        public int CompletedApplications { get; set; }


        // COMMUNITY
        public int UniqueVolunteers { get; set; }

        public int TotalShifts { get; set; }

        public int TotalCapacity { get; set; }

        public int ApprovedPlaces { get; set; }


        // ACTION BREAKDOWN
        public List<OrganizerActionStatisticsViewModel> Actions { get; set; }
            = new List<OrganizerActionStatisticsViewModel>();
    }


    public class OrganizerActionStatisticsViewModel
    {
        public int ActionId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public int Shifts { get; set; }

        public int Applications { get; set; }

        public int Approved { get; set; }

        public int Pending { get; set; }

        public int Completed { get; set; }
    }
}