using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VolunteerHub.Models;

namespace VolunteerHub.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<VolunteerAction> VolunteerActions { get; set; }

        public DbSet<Shift> Shifts { get; set; }

        public DbSet<ParticipationRequest> ParticipationRequests { get; set; }

        public DbSet<Country> Countries { get; set; }

        public DbSet<City> Cities { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            // COUNTRY
            builder.Entity<Country>()
                .HasIndex(c => c.Code)
                .IsUnique();

            builder.Entity<Country>()
                .HasIndex(c => c.Name)
                .IsUnique();


            // CITY
            builder.Entity<City>()
                .HasOne(c => c.Country)
                .WithMany(c => c.Cities)
                .HasForeignKey(c => c.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<City>()
                .HasIndex(c => new
                {
                    c.CountryId,
                    c.Name
                })
                .IsUnique();


            // USER LOCATION
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.City)
                .WithMany()
                .HasForeignKey(u => u.CityId)
                .OnDelete(DeleteBehavior.Restrict);


            // ACTION LOCATION
            builder.Entity<VolunteerAction>()
                .HasOne(a => a.City)
                .WithMany()
                .HasForeignKey(a => a.CityId)
                .OnDelete(DeleteBehavior.Restrict);


            // ACTION ORGANIZER
            builder.Entity<VolunteerAction>()
                .HasOne(a => a.Organizer)
                .WithMany()
                .HasForeignKey(a => a.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);


            // SHIFT
            builder.Entity<Shift>()
                .HasOne(s => s.VolunteerAction)
                .WithMany(a => a.Shifts)
                .HasForeignKey(s => s.VolunteerActionId)
                .OnDelete(DeleteBehavior.Cascade);


            // PARTICIPATION REQUEST - VOLUNTEER
            builder.Entity<ParticipationRequest>()
                .HasOne(r => r.Volunteer)
                .WithMany()
                .HasForeignKey(r => r.VolunteerId)
                .OnDelete(DeleteBehavior.Restrict);


            // PARTICIPATION REQUEST - SHIFT
            builder.Entity<ParticipationRequest>()
                .HasOne(r => r.Shift)
                .WithMany(s => s.ParticipationRequests)
                .HasForeignKey(r => r.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);


            // COUNTRY SEED DATA
            builder.Entity<Country>().HasData(
                new Country
                {
                    Id = 1,
                    Name = "Greece",
                    Code = "GR",
                    TimeZoneId = "Europe/Athens"
                },
                new Country
                {
                    Id = 2,
                    Name = "Switzerland",
                    Code = "CH",
                    TimeZoneId = "Europe/Zurich"
                },
                new Country
                {
                    Id = 3,
                    Name = "Germany",
                    Code = "DE",
                    TimeZoneId = "Europe/Berlin"
                },
                new Country
                {
                    Id = 4,
                    Name = "Austria",
                    Code = "AT",
                    TimeZoneId = "Europe/Vienna"
                },
                new Country
                {
                    Id = 5,
                    Name = "France",
                    Code = "FR",
                    TimeZoneId = "Europe/Paris"
                },
                new Country
                {
                    Id = 6,
                    Name = "Italy",
                    Code = "IT",
                    TimeZoneId = "Europe/Rome"
                },
                new Country
                {
                    Id = 7,
                    Name = "Spain",
                    Code = "ES",
                    TimeZoneId = "Europe/Madrid"
                },
                new Country
                {
                    Id = 8,
                    Name = "Netherlands",
                    Code = "NL",
                    TimeZoneId = "Europe/Amsterdam"
                },
                new Country
                {
                    Id = 9,
                    Name = "Belgium",
                    Code = "BE",
                    TimeZoneId = "Europe/Brussels"
                },
                new Country
                {
                    Id = 10,
                    Name = "United Kingdom",
                    Code = "GB",
                    TimeZoneId = "Europe/London"
                }
            );


            // CITY SEED DATA
            builder.Entity<City>().HasData(

                // Greece
                new City { Id = 1, Name = "Athens", CountryId = 1 },
                new City { Id = 2, Name = "Thessaloniki", CountryId = 1 },
                new City { Id = 3, Name = "Patras", CountryId = 1 },
                new City { Id = 4, Name = "Heraklion", CountryId = 1 },
                new City { Id = 5, Name = "Larissa", CountryId = 1 },
                new City { Id = 6, Name = "Volos", CountryId = 1 },
                new City { Id = 7, Name = "Ioannina", CountryId = 1 },
                new City { Id = 8, Name = "Kavala", CountryId = 1 },
                new City { Id = 9, Name = "Kalamata", CountryId = 1 },
                new City { Id = 10, Name = "Serres", CountryId = 1 },

                // Switzerland
                new City { Id = 11, Name = "Zurich", CountryId = 2 },
                new City { Id = 12, Name = "Geneva", CountryId = 2 },
                new City { Id = 13, Name = "Basel", CountryId = 2 },
                new City { Id = 14, Name = "Bern", CountryId = 2 },
                new City { Id = 15, Name = "Lausanne", CountryId = 2 },
                new City { Id = 16, Name = "Lucerne", CountryId = 2 },
                new City { Id = 17, Name = "Winterthur", CountryId = 2 },
                new City { Id = 18, Name = "St. Gallen", CountryId = 2 },
                new City { Id = 19, Name = "Zug", CountryId = 2 },
                new City { Id = 20, Name = "Lugano", CountryId = 2 },

                // Germany
                new City { Id = 21, Name = "Berlin", CountryId = 3 },
                new City { Id = 22, Name = "Hamburg", CountryId = 3 },
                new City { Id = 23, Name = "Munich", CountryId = 3 },
                new City { Id = 24, Name = "Cologne", CountryId = 3 },
                new City { Id = 25, Name = "Frankfurt", CountryId = 3 },
                new City { Id = 26, Name = "Stuttgart", CountryId = 3 },

                // Austria
                new City { Id = 27, Name = "Vienna", CountryId = 4 },
                new City { Id = 28, Name = "Graz", CountryId = 4 },
                new City { Id = 29, Name = "Linz", CountryId = 4 },
                new City { Id = 30, Name = "Salzburg", CountryId = 4 },
                new City { Id = 31, Name = "Innsbruck", CountryId = 4 },

                // France
                new City { Id = 32, Name = "Paris", CountryId = 5 },
                new City { Id = 33, Name = "Marseille", CountryId = 5 },
                new City { Id = 34, Name = "Lyon", CountryId = 5 },
                new City { Id = 35, Name = "Toulouse", CountryId = 5 },
                new City { Id = 36, Name = "Nice", CountryId = 5 },

                // Italy
                new City { Id = 37, Name = "Rome", CountryId = 6 },
                new City { Id = 38, Name = "Milan", CountryId = 6 },
                new City { Id = 39, Name = "Naples", CountryId = 6 },
                new City { Id = 40, Name = "Turin", CountryId = 6 },
                new City { Id = 41, Name = "Florence", CountryId = 6 },

                // Spain
                new City { Id = 42, Name = "Madrid", CountryId = 7 },
                new City { Id = 43, Name = "Barcelona", CountryId = 7 },
                new City { Id = 44, Name = "Valencia", CountryId = 7 },
                new City { Id = 45, Name = "Seville", CountryId = 7 },
                new City { Id = 46, Name = "Malaga", CountryId = 7 },

                // Netherlands
                new City { Id = 47, Name = "Amsterdam", CountryId = 8 },
                new City { Id = 48, Name = "Rotterdam", CountryId = 8 },
                new City { Id = 49, Name = "The Hague", CountryId = 8 },
                new City { Id = 50, Name = "Utrecht", CountryId = 8 },

                // Belgium
                new City { Id = 51, Name = "Brussels", CountryId = 9 },
                new City { Id = 52, Name = "Antwerp", CountryId = 9 },
                new City { Id = 53, Name = "Ghent", CountryId = 9 },
                new City { Id = 54, Name = "Bruges", CountryId = 9 },

                // United Kingdom
                new City { Id = 55, Name = "London", CountryId = 10 },
                new City { Id = 56, Name = "Manchester", CountryId = 10 },
                new City { Id = 57, Name = "Birmingham", CountryId = 10 },
                new City { Id = 58, Name = "Liverpool", CountryId = 10 },
                new City { Id = 59, Name = "Edinburgh", CountryId = 10 },
                new City { Id = 60, Name = "Glasgow", CountryId = 10 }
            );
        }
    }
}