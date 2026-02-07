# Practical 20 - Student Management (ASP.NET Core MVC)

## Overview
This application is an ASP.NET Core MVC project that demonstrates:
- Global exception handling using middleware.
- Auditing for data changes (create/update/delete) stored in the database.
- Application logging written to the database with a file backup.
- A generic repository with the Unit of Work pattern.
- Async-first controller and data access flows.

## Solution Structure
- **Practical-20/**: ASP.NET Core MVC project
  - **Controllers/**: MVC controllers (Student CRUD flow).
  - **Data/**: `AppDbContext` with auditing support.
  - **Logging/**: Database logger with file fallback.
  - **Middlewares/**: Global exception handling middleware.
  - **Repositories/**: Generic repository and Unit of Work.
  - **Models/**: Entities (Student, LogEntry, AuditEntry).

## Prerequisites
- .NET SDK 6.0+
- SQL Server instance

## Configuration
Update the connection string in `Practical-20/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=Practical20Db;Trusted_Connection=True;TrustServerCertificate=True"
}
```

## Database Setup
From the solution root:

```bash
dotnet ef database update --project Practical-20/Practical-20.csproj
```

## Run the Application
From the solution root:

```bash
dotnet run --project Practical-20/Practical-20.csproj
```

## Global Exception Handling
The `ExceptionHandlingMiddleware` is registered in `Program.cs` to handle unhandled exceptions consistently and return JSON error responses.

## Auditing (Database)
Auditing is implemented inside `AppDbContext`. Any insert/update/delete (excluding log and audit tables) produces an `AuditEntry` that captures:
- Action (Added/Modified/Deleted)
- Table name
- Record Id
- Old values
- New values
- User (when available)
- Timestamp

## Logging (Database + File Backup)
Logging is configured with a database logger provider. Logs are written to the `LogEntries` table.  
If the database write fails, the log is written to `logs/app.log` as a backup.

## Generic Repository + Unit of Work
The repository pattern abstracts common CRUD operations and the Unit of Work coordinates saving changes:
- `IRepository<T>` / `Repository<T>`
- `IUnitOfWork` / `UnitOfWork`

All controller actions use asynchronous repository methods to keep the request pipeline responsive.

## Debugging the Application Flow
To practice debugging (breakpoints, watches, step-through, inspection):

1. Open the solution (`Practical-20.sln`) in Visual Studio or Rider.
2. Set breakpoints in:
   - `StudentsController` actions (Index, Create, Edit, Delete).
   - `AppDbContext.SaveChangesAsync` (audit entry creation).
   - `ExceptionHandlingMiddleware`.
3. Run the app in **Debug** mode.
4. Use **Watch/Locals** to inspect entities, `ChangeTracker` entries, and controller inputs.
5. Trigger an exception by calling `/Students/ThrowTestException` to observe the middleware behavior.
6. Step into repository methods to see how data flows through the Unit of Work.

## Notes
- Ensure the database is reachable before running.
- Logs are persisted to the database; the file fallback is only used when database writes fail.
