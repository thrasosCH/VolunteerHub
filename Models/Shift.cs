using System.ComponentModel.DataAnnotations;

namespace VolunteerHub.Models
{
    public class Shift
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Shift Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1500)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [Range(
            1,
            1000,
            ErrorMessage = "The maximum number of volunteers must be between 1 and 1000.")]
        [Display(Name = "Maximum Volunteers")]
        public int MaxVolunteers { get; set; }

        [StringLength(250)]
        [Display(Name = "Required Skill")]
        public string? RequiredSkill { get; set; }

        [StringLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        public int VolunteerActionId { get; set; }

        public VolunteerAction VolunteerAction { get; set; } = null!;

        public ICollection<ParticipationRequest> ParticipationRequests { get; set; }
            = new List<ParticipationRequest>();
    }
}