# Appointment Booking System

ASP.NET Core Web API + EF Core backend, Angular frontend, headed for Azure App Service + Azure SQL.

```
BookingSystem/
├── BookingSystem.sln
└── src/
    ├── BookingSystem.Api/         C# backend      -> open in Visual Studio
    └── booking-system-client/     Angular frontend -> open in VS Code
```

## Daily workflow (two editors, one repo)

| Part | Editor | How to open |
|------|--------|-------------|
| C# API | Visual Studio 2022 Community | File > Open > Project/Solution > `BookingSystem.sln` |
| Angular | VS Code | `code src/booking-system-client` (or File > Open Folder) |

Keep both open together. Run the API from Visual Studio, run Angular from the VS Code terminal.

## First-time setup

### 1. API (Visual Studio)
1. Install Visual Studio 2022 Community with the **ASP.NET and web development** workload
   (this includes SQL Server LocalDB).
2. Open `BookingSystem.sln`. Let NuGet restore finish.
3. Open **Tools > NuGet Package Manager > Package Manager Console**, make sure the
   *Default project* dropdown says `BookingSystem.Api`, then run:
   ```
   Add-Migration InitialCreate
   Update-Database
   ```
   This creates the `BookingSystemDb` database in LocalDB with the seeded providers/services.
4. In the toolbar pick the **http** launch profile and press **F5**.
   Swagger opens at http://localhost:5017/swagger.

### 2. Angular (VS Code)
1. Install [Node.js 20+](https://nodejs.org).
2. Open `src/booking-system-client` in VS Code (accept the recommended Angular extension).
3. In the integrated terminal:
   ```
   npm install
   npm start
   ```
4. Open http://localhost:4200 - with the API running you should see the seeded services and providers.

## GitHub

From the repo root (the folder containing `BookingSystem.sln`):

```
git init
git add .
git commit -m "Initial commit: booking system API and Angular client"
git branch -M main
git remote add origin https://github.com/<your-username>/<repo-name>.git
git push -u origin main
```

Create the empty repo on github.com first (no README/.gitignore/license - this folder has them).
Visual Studio users can do the same from the **Git** menu > *Create Git Repository*.

Never commit real secrets. Local LocalDB settings are fine; when you move to Azure SQL, keep the
password out of `appsettings.json` (use User Secrets locally, App Service configuration in Azure).

## What's built so far
- Models, `AppDbContext`, and controllers for services, providers, availability and bookings (FR-1 to FR-4).
- `POST /api/availability` to publish new slots, plus a development seeder that tops up hourly
  slots (Mon-Fri, next 7 days) every time the API starts.
- Angular booking flow: choose service, choose provider, pick a time, enter details, confirm, cancel.

## After pulling these changes
In the Package Manager Console run `Add-Migration FilteredSlotIndex` then `Update-Database`
(the unique index on bookings now ignores cancelled bookings so a freed slot can be rebooked).

## Next
1. Admin screen in Angular to add/remove availability slots.
2. Email confirmation (FR-6).
3. Azure SQL, then App Service deploy with GitHub Actions.
