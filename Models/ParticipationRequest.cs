using System.ComponentModel.DataAnnotations;
using VolunteerHub.Constants;

namespace VolunteerHub.Models
{
    public class ParticipationRequest
    {
        public int Id { get; set; }


        [Required]
        public string VolunteerId { get; set; } = string.Empty;

        public ApplicationUser Volunteer { get; set; } = null!;


        public int ShiftId { get; set; }

        public Shift Shift { get; set; } = null!;


        [Display(Name = "Application Date")]
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;


        [StringLength(1000)]
        [Display(Name = "Message to Organizer")]
        public string? Message { get; set; }


        [Required]
        [StringLength(30)]
        [Display(Name = "Status")]
        public string Status { get; set; }
            = ParticipationRequestStatuses.Pending;


        [StringLength(1000)]
        [Display(Name = "Organizer Notes")]
        public string? OrganizerNotes { get; set; }


        [Display(Name = "Attended")]
        public bool Attended { get; set; } = false;
    }
}