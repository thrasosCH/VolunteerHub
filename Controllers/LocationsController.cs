using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerHub.Data;

namespace VolunteerHub.Controllers
{
    public class LocationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LocationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetCities(int countryId)
        {
            var cities = await _context.Cities
                .Where(c => c.CountryId == countryId)
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name
                })
                .ToListAsync();

            return Json(cities);
        }
    }
}