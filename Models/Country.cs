using System.ComponentModel.DataAnnotations;

namespace VolunteerHub.Models
{
    public class Country
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(2)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string TimeZoneId { get; set; } = string.Empty;

        public ICollection<City> Cities { get; set; }
            = new List<City>();
    }
}