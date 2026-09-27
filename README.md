# VolunteerHub

VolunteerHub is a web application for managing volunteer activities.

The application allows volunteers to find activities and apply for available shifts, while organizers can create activities, manage shifts and review participation requests.

This project was developed using ASP.NET Core MVC.

## Technologies

- C#
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server LocalDB
- ASP.NET Core Identity
- Bootstrap
- HTML / CSS

## User Roles

### Volunteer

A volunteer can:

- Create an account and login
- View and edit their profile
- Search available volunteer activities
- Filter activities by category, location and date
- View available shifts
- Apply for a shift
- Cancel an application
- View upcoming participations
- View participation history
- View their dashboard

### Organizer

An organizer can:

- Create volunteer activities
- Edit their own activities
- Publish or cancel activities
- Change activity status
- Create and manage shifts
- View volunteer applications
- Approve or reject applications
- Record volunteer attendance
- View an organizer dashboard
- View activity statistics

### Admin

An administrator can:

- View system statistics
- View registered users
- View user roles
- Manage users
- View all volunteer activities
- Delete activities

## Activity Statuses

Activities can have the following statuses:

- Draft
- Published
- InProgress
- Completed
- Cancelled

## Participation Request Statuses

Participation requests can have the following statuses:

- Pending
- Approved
- Rejected
- Cancelled
- Completed
- Waitlist

## Business Rules

The application includes several basic validation rules:

- Volunteers cannot apply after the application deadline
- Volunteers cannot create duplicate active applications for the same shift
- A shift cannot exceed its maximum number of approved volunteers
- A volunteer cannot be approved for overlapping shifts
- Organizers can only manage their own activities
- Only volunteers can submit participation requests
- Only organizers can manage applications for their activities



The project also includes:

- Organizer statistics
- Waitlist system for full shifts
- Admin role
- Admin dashboard
- User management
- Activity management

## Database

The project uses Entity Framework Core with SQL Server LocalDB.

Database migrations are included in:

`Data/Migrations`

To create or update the database, use the Package Manager Console in Visual Studio:

```powershell
Update-Database