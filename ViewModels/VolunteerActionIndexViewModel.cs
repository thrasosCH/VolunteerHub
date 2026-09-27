using Microsoft.AspNetCore.Mvc.Rendering;
using VolunteerHub.Models;

namespace VolunteerHub.ViewModels
{
    public class VolunteerActionIndexViewModel
    {
        public string? SearchString { get; set; }

        public string? Category { get; set; }

        public int? CountryId { get; set; }

        public int? CityId { get; set; }

        public DateTime? Date { get; set; }

        public List<VolunteerAction> Actions { get; set; }
            = new List<VolunteerAction>();

        public List<SelectListItem> Categories { get; set; }
            = new List<SelectListItem>();

        public List<SelectListItem> Countries { get; set; }
            = new List<SelectListItem>();

        public List<SelectListItem> Cities { get; set; }
            = new List<SelectListItem>();
    }
}