using System.ComponentModel.DataAnnotations;
using VolunteerHub.Constants;

namespace VolunteerHub.Models
{
    public class VolunteerAction
    {
        public int Id { get; set; }


        [Required]
        [StringLength(150)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;


        [Required]
        [StringLength(3000)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;


        [Required]
        [StringLength(100)]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;


        // -------------------------------------------------
        // LOCATION
        // -------------------------------------------------

        public int? CityId { get; set; }

        public City? City { get; set; }


        // -------------------------------------------------
        // DATES
        // -------------------------------------------------

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }


        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }


        [Display(Name = "Application Deadline")]
        public DateTime ApplicationDeadline { get; set; }


        // -------------------------------------------------
        // CONTACT
        // -------------------------------------------------

        [Required]
        [StringLength(250)]
        [Display(Name = "Contact Information")]
        public string ContactInfo { get; set; } = string.Empty;


        // -------------------------------------------------
        // STATUS
        // -------------------------------------------------

        [Required]
        [StringLength(30)]
        public string Status { get; set; }
            = VolunteerActionStatuses.Draft;


        // -------------------------------------------------
        // OPTIONAL DETAILS
        // -------------------------------------------------

        [Display(Name = "Minimum Age")]
        [Range(16, 100)]
        public int? MinimumAge { get; set; }


        [StringLength(2000)]
        [Display(Name = "Additional Instructions")]
        public string? Instructions { get; set; }


        // -------------------------------------------------
        // ORGANIZER
        // -------------------------------------------------

        [Required]
        public string OrganizerId { get; set; } = string.Empty;


        public ApplicationUser Organizer { get; set; } = null!;


        // -------------------------------------------------
        // SHIFTS
        // -------------------------------------------------

        public ICollection<Shift> Shifts { get; set; }
            = new List<Shift>();
    }
}