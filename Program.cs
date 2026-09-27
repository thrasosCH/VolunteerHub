using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VolunteerHub.Constants;
using VolunteerHub.Data;
using VolunteerHub.Models;
using VolunteerHub.Services;

var builder = WebApplication.CreateBuilder(args);


// ---------------------------------------------------------
// DATABASE
// ---------------------------------------------------------

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();


// ---------------------------------------------------------
// IDENTITY
// ---------------------------------------------------------

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();


// ---------------------------------------------------------
// SERVICES
// ---------------------------------------------------------

builder.Services.AddSingleton<ITimeService, TimeService>();


// ---------------------------------------------------------
// MVC
// ---------------------------------------------------------

builder.Services.AddControllersWithViews();


var app = builder.Build();


// ---------------------------------------------------------
// CREATE ROLES + OPTIONAL ADMIN ACCOUNT
// ---------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var services =
        scope.ServiceProvider;

    var roleManager =
        services.GetRequiredService<
            RoleManager<IdentityRole>>();

    var userManager =
        services.GetRequiredService<
            UserManager<ApplicationUser>>();


    // -----------------------------------------------------
    // CREATE APPLICATION ROLES
    // -----------------------------------------------------

    string[] roles =
    {
        UserRoles.Volunteer,
        UserRoles.Organizer,
        UserRoles.Admin
    };


    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var roleResult =
                await roleManager.CreateAsync(
                    new IdentityRole(role));


            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    Console.WriteLine(
                        $"Role creation error: {error.Description}");
                }
            }
        }
    }


    // -----------------------------------------------------
    // ADMIN SEED CONFIGURATION
    // -----------------------------------------------------

    var adminEmail =
        builder.Configuration[
            "SeedAdmin:Email"];

    var adminPassword =
        builder.Configuration[
            "SeedAdmin:Password"];


    // -----------------------------------------------------
    // CREATE ADMIN ONLY WHEN CREDENTIALS ARE CONFIGURED
    // -----------------------------------------------------

    if (!string.IsNullOrWhiteSpace(adminEmail) &&
        !string.IsNullOrWhiteSpace(adminPassword))
    {
        var adminUser =
            await userManager.FindByEmailAsync(
                adminEmail);


        if (adminUser == null)
        {
            adminUser =
                new ApplicationUser
                {
                    UserName =
                        adminEmail,

                    Email =
                        adminEmail,

                    EmailConfirmed =
                        true,

                    FullName =
                        "VolunteerHub Admin",

                    Bio =
                        "System Administrator",

                    Skills =
                        "Administration",

                    RegistrationDate =
                        DateTime.UtcNow,

                    // Thessaloniki
                    CityId =
                        2
                };


            var createResult =
                await userManager.CreateAsync(
                    adminUser,
                    adminPassword);


            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    Console.WriteLine(
                        $"Admin creation error: {error.Description}");
                }


                adminUser =
                    null;
            }
        }


        if (adminUser != null &&
            !await userManager.IsInRoleAsync(
                adminUser,
                UserRoles.Admin))
        {
            var roleResult =
                await userManager.AddToRoleAsync(
                    adminUser,
                    UserRoles.Admin);


            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    Console.WriteLine(
                        $"Admin role error: {error.Description}");
                }
            }
        }
    }
    else
    {
        Console.WriteLine(
            "Admin seed skipped. SeedAdmin credentials are not configured.");
    }
}


// ---------------------------------------------------------
// HTTP PIPELINE
// ---------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler(
        "/Home/Error");

    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();


// ---------------------------------------------------------
// MVC ROUTES
// ---------------------------------------------------------

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


// ---------------------------------------------------------
// IDENTITY RAZOR PAGES
// ---------------------------------------------------------

app.MapRazorPages()
    .WithStaticAssets();


app.Run();