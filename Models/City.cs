using System.ComponentModel.DataAnnotations;

namespace VolunteerHub.Models
{
    public class City
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        public int CountryId { get; set; }

        public Country Country { get; set; } = null!;
    }
}