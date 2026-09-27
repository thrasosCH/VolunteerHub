using Microsoft.AspNetCore.Identity;
using VolunteerHub.Constants;
using VolunteerHub.Models;

namespace VolunteerHub.Data
{
    public static class ApplicationDbInitializer
    {
        public static async Task InitializeAsync(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger logger)
        {
            using var scope =
                serviceProvider.CreateScope();

            var services =
                scope.ServiceProvider;


            var roleManager =
                services.GetRequiredService<
                    RoleManager<IdentityRole>>();

            var userManager =
                services.GetRequiredService<
                    UserManager<ApplicationUser>>();


            // -------------------------------------------------
            // CREATE APPLICATION ROLES
            // -------------------------------------------------

            await CreateRolesAsync(
                roleManager,
                logger);


            // -------------------------------------------------
            // ADMIN ACCOUNT
            // -------------------------------------------------

            await SeedUserAsync(
                userManager: userManager,
                configuration: configuration,
                logger: logger,

                configurationSection:
                    "SeedAdmin",

                role:
                    UserRoles.Admin,

                fullName:
                    "VolunteerHub Admin",

                bio:
                    "System Administrator",

                skills:
                    "Administration",

                cityId:
                    2);


            // -------------------------------------------------
            // DEMO ORGANIZER
            // -------------------------------------------------

            await SeedUserAsync(
                userManager: userManager,
                configuration: configuration,
                logger: logger,

                configurationSection:
                    "SeedDemoOrganizer",

                role:
                    UserRoles.Organizer,

                fullName:
                    "Demo Organizer",

                bio:
                    "Demo organizer account for exploring VolunteerHub.",

                skills:
                    "Community Management, Event Coordination",

                // Zurich
                cityId:
                    11);


            // -------------------------------------------------
            // DEMO VOLUNTEER
            // -------------------------------------------------

            await SeedUserAsync(
                userManager: userManager,
                configuration: configuration,
                logger: logger,

                configurationSection:
                    "SeedDemoVolunteer",

                role:
                    UserRoles.Volunteer,

                fullName:
                    "Demo Volunteer",

                bio:
                    "Demo volunteer account for exploring VolunteerHub.",

                skills:
                    "Community Support, Environment",

                // Zurich
                cityId:
                    11);
        }


        // -------------------------------------------------
        // CREATE ROLES
        // -------------------------------------------------

        private static async Task CreateRolesAsync(
            RoleManager<IdentityRole> roleManager,
            ILogger logger)
        {
            string[] roles =
            {
                UserRoles.Volunteer,
                UserRoles.Organizer,
                UserRoles.Admin
            };


            foreach (var role in roles)
            {
                if (await roleManager.RoleExistsAsync(role))
                {
                    continue;
                }


                var roleResult =
                    await roleManager.CreateAsync(
                        new IdentityRole(role));


                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        logger.LogError(
                            "Role creation error for {Role}: {Description}",
                            role,
                            error.Description);
                    }
                }
            }
        }


        // -------------------------------------------------
        // SEED USER
        // -------------------------------------------------

        private static async Task SeedUserAsync(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ILogger logger,
            string configurationSection,
            string role,
            string fullName,
            string bio,
            string skills,
            int cityId)
        {
            var email =
                configuration[
                    $"{configurationSection}:Email"];


            var password =
                configuration[
                    $"{configurationSection}:Password"];


            // -------------------------------------------------
            // SKIP WHEN CREDENTIALS ARE NOT CONFIGURED
            // -------------------------------------------------

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                logger.LogInformation(
                    "{ConfigurationSection} seed skipped because credentials are not configured.",
                    configurationSection);

                return;
            }


            // -------------------------------------------------
            // FIND EXISTING USER
            // -------------------------------------------------

            var user =
                await userManager.FindByEmailAsync(
                    email);


            // -------------------------------------------------
            // CREATE USER
            // -------------------------------------------------

            if (user == null)
            {
                user =
                    new ApplicationUser
                    {
                        UserName =
                            email,

                        Email =
                            email,

                        EmailConfirmed =
                            true,

                        FullName =
                            fullName,

                        Bio =
                            bio,

                        Skills =
                            skills,

                        RegistrationDate =
                            DateTime.UtcNow,

                        CityId =
                            cityId
                    };


                var createResult =
                    await userManager.CreateAsync(
                        user,
                        password);


                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                    {
                        logger.LogError(
                            "User creation error for {Email}: {Description}",
                            email,
                            error.Description);
                    }


                    return;
                }


                logger.LogInformation(
                    "Seed user {Email} was created successfully.",
                    email);
            }


            // -------------------------------------------------
            // ENSURE CORRECT ROLE
            // -------------------------------------------------

            if (!await userManager.IsInRoleAsync(
                user,
                role))
            {
                var roleResult =
                    await userManager.AddToRoleAsync(
                        user,
                        role);


                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        logger.LogError(
                            "Role assignment error for {Email}: {Description}",
                            email,
                            error.Description);
                    }
                }
            }
        }
    }
}