using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VolunteerHub.ViewModels
{
    public class VolunteerActionFormViewModel
    {
        public int Id { get; set; }


        // -------------------------------------------------
        // BASIC INFORMATION
        // -------------------------------------------------

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

        [Required(ErrorMessage = "Please select a country.")]
        [Display(Name = "Country")]
        public int? CountryId { get; set; }


        [Required(ErrorMessage = "Please select a city.")]
        [Display(Name = "City")]
        public int? CityId { get; set; }


        // -------------------------------------------------
        // SCHEDULE
        // -------------------------------------------------

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }


        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }


        [Required]
        [Display(Name = "Application Deadline")]
        public DateTime ApplicationDeadline { get; set; }


        // -------------------------------------------------
        // ADDITIONAL DETAILS
        // -------------------------------------------------

        [Required]
        [StringLength(250)]
        [Display(Name = "Contact Information")]
        public string ContactInfo { get; set; } = string.Empty;


        [Display(Name = "Minimum Age")]
        [Range(16, 100)]
        public int? MinimumAge { get; set; }


        [StringLength(2000)]
        [Display(Name = "Additional Instructions")]
        public string? Instructions { get; set; }


        // -------------------------------------------------
        // DROPDOWN OPTIONS
        // -------------------------------------------------

        public List<SelectListItem> Countries { get; set; }
            = new();


        public List<SelectListItem> Cities { get; set; }
            = new();


        public List<SelectListItem> Categories { get; set; }
            = new();
    }
}