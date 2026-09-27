using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VolunteerHub.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;


        [StringLength(500)]
        public string Bio { get; set; } = string.Empty;


        [StringLength(500)]
        public string Skills { get; set; } = string.Empty;


        public DateTime RegistrationDate { get; set; }
            = DateTime.UtcNow;


        // -------------------------------------------------
        // LOCATION
        // -------------------------------------------------

        public int? CityId { get; set; }

        public City? City { get; set; }
    }
}