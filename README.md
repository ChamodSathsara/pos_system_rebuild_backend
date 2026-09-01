# POS System API

Production-ready ASP.NET Core 8 Web API for a Point-of-Sale system, built with a single-project
layered architecture (Controllers → Services → Repositories → EF Core/SQL Server).

## Architecture

```
Controllers/        Thin HTTP controllers - no business logic
Data/                ApplicationDbContext, EF entity configurations, repository implementations, seeder
Repository/          Repository & Unit of Work INTERFACES only
Service/             Business logic (Service/Interfaces + implementations)
Security/            Password hashing (BCrypt) + JWT token generation
Migrations/          EF Core migrations (generate with `dotnet ef migrations add`)
Models/Entities/     EF Core entity classes (one file per domain area)
Models/Enums/        All enums from the schema
DTOs/                Request/response contracts, grouped by feature
Mappings/            AutoMapper profiles
Middleware/          Global exception handling
Exceptions/          Typed application exceptions (404/400/409/401/403/422)
Validators/          FluentValidation validators
Common/               ApiResponse<T> envelope
Helpers/ Extensions/  Cross-cutting helpers (ClaimsPrincipal extensions, DI registration)
Constants/            Role names & JWT claim name constants
Configuration/        Strongly-typed settings (JwtSettings)
```

## Key design decisions

- **`user_group` → `user_role`**: The original `user_group` / `user_group_permission` tables are
  fully renamed to `user_role` / `user_role_permission` throughout the entities, EF configuration,
  DbContext, repositories, JWT claims (`role`), and seed data. `system_user.group_id` became
  `system_user.RoleId` / column `role_id`.
- **JWT claims**: every access token includes `user_id` (system_user.user_code), `role`
  (user_role.role_name) and the username (`ClaimTypes.Name`) plus email when available.
- **Repository layer**: `Repository/` holds only interfaces; concrete implementations live under
  `Data/Repositories/` since they are part of the data-access layer.
- **Password hashing**: BCrypt (`BCrypt.Net-Next`), work factor 12.
- **Global error handling**: `Middleware/ExceptionHandlingMiddleware` converts every thrown
  exception (including FluentValidation's `ValidationException`) into a consistent
  `ApiResponse<T>` JSON payload with the correct HTTP status code.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (local, container, or Azure SQL) reachable via the connection string in
  `appsettings.json` / `appsettings.Development.json`

## Solution structure

This repo uses the new **`.slnx`** XML solution format (default in the .NET 10 SDK) instead of
the legacy `.sln` format:

```
PosApi.slnx    <- solution file (XML), references PosApi.csproj
PosApi.csproj  <- targets net10.0
```

Open it with `dotnet` CLI, Visual Studio 2022 17.14+, or Rider (all support `.slnx` natively).
Add more projects (e.g. a test project) with:
```bash
dotnet sln PosApi.slnx add path/to/OtherProject.csproj
```

## Getting started

1. **Restore packages**
   ```bash
   dotnet restore
   ```

2. **Configure secrets** (recommended over editing appsettings.json directly)
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=PosSystemDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
   dotnet user-secrets set "Jwt:Secret" "<a long random string, at least 32 characters>"
   ```

3. **Create the initial migration** (the `Migrations/` folder is intentionally empty in this
   generated project - EF's `dotnet ef` tool needs to run in an environment with NuGet package
   access, so run this once locally):
   ```bash
   dotnet tool install --global dotnet-ef   # if not already installed
   dotnet ef migrations add InitialCreate
   ```

4. **Run the API** - migrations are applied automatically on startup
   (`ApplyMigrationsOnStartup: true` in appsettings.json), and a default admin user plus baseline
   roles are seeded the first time the app runs:
   ```bash
   dotnet run
   ```
   Swagger UI opens at `https://localhost:5443/swagger` (or `http://localhost:5080/swagger`).

   Alternatively apply migrations explicitly without seeding via startup:
   ```bash
   dotnet ef database update
   ```

## Default seeded account

| Username | Password   | Role  |
|----------|-----------|-------|
| `admin`  | `Admin@123` | Admin |

**Change this password immediately in any non-local environment.**

## API overview

### Auth (`/api/auth`)
| Method | Route            | Auth | Description |
|--------|-------------------|------|--------------|
| POST   | `/login`          | none | Authenticate, returns access + refresh token |
| POST   | `/logout`         | JWT  | Revokes refresh token(s) for the caller |
| GET    | `/me`             | JWT  | Returns the authenticated user's profile |
| GET    | `/test`           | JWT  | Smoke-test route confirming JWT auth works |

### Customers (`/api/customers`)
| Method | Route  | Auth | Description |
|--------|--------|------|--------------|
| POST   | `/`    | JWT  | Create a new customer |

All responses are wrapped in the standard envelope:
```json
{
  "success": true,
  "message": "Request successful",
  "data": { },
  "errors": null,
  "timestamp": "2026-08-19T00:00:00Z"
}
```

## Example requests

**Login**
```http
POST /api/auth/login
Content-Type: application/json

{ "username": "admin", "password": "Admin@123" }
```

**Create customer**
```http
POST /api/customers
Authorization: Bearer <accessToken>
Content-Type: application/json

{
  "customerName": "Jane Doe",
  "mobile": "+94771234567",
  "email": "jane@example.com",
  "customerType": "Regular",
  "creditLimit": 0
}
```

## Extending the project

- Add new repositories under `Repository/` (interface) and `Data/Repositories/` (implementation),
  then register them in `Extensions/ServiceCollectionExtensions.AddRepositories`.
- Add new services under `Service/Interfaces` and `Service/`, then register them in
  `AddApplicationServices`.
- Add new controllers under `Controllers/`, inheriting `BaseApiController` for access to
  `CurrentUserCode`.
- All 40 tables from the source schema already have EF entities, configurations and DbSets, so
  wiring up additional CRUD endpoints (products, sales, GRN, stock, etc.) only requires adding the
  Service/Controller/DTO layers on top of the existing data layer.

## Troubleshooting

**Build fails with `CS7069`/`CS0234` errors mentioning `Microsoft.OpenApi` / `OpenApiInfo` /
`OpenApiSecurityScheme` in `ServiceCollectionExtensions.cs`:**

This happens if the project references both `Swashbuckle.AspNetCore` (which uses
`Microsoft.OpenApi` 1.x) and `Microsoft.AspNetCore.OpenApi` (the built-in .NET OpenAPI generator,
which pulls in `Microsoft.OpenApi` 2.x). NuGet unifies the two packages to one `Microsoft.OpenApi`
version, and Swashbuckle's compiled code can't find the 1.x-shaped types it expects, so the
compiler reports the type as "defined in an assembly that could not be found."

This project doesn't use the built-in generator (no `AddOpenApi()`/`MapOpenApi()` calls anywhere),
so the fix is to make sure `Microsoft.AspNetCore.OpenApi` is **not** referenced in `PosApi.csproj`
— only `Swashbuckle.AspNetCore` plus the defensive `Microsoft.OpenApi` 1.x pin already present. If
you added the built-in package back in (e.g. from a template), remove it, then:
```bash
dotnet nuget locals all --clear
dotnet restore
```

**App builds and runs, but crashes on startup with `Microsoft.Data.SqlClient.SqlException: A
network-related or instance-specific error occurred... TCP Provider, error 0 - The wait operation
timed out`:**

This is not a code issue — it means no SQL Server is reachable at the address in
`ConnectionStrings:DefaultConnection`. It fails inside `DbSeeder.SeedAsync` because
`ApplyMigrationsOnStartup` (in `appsettings.json`) makes the app call
`context.Database.MigrateAsync()` on every startup, which needs a live connection. Pick one:

- **LocalDB (Windows / Visual Studio, no install needed)** — this is the default in
  `appsettings.Development.json`:
  ```
  Server=(localdb)\mssqllocaldb;Database=PosSystemDb_Dev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
  ```
  LocalDB starts itself automatically on first connection — nothing to run manually. Confirm it's
  installed with `sqllocaldb info mssqllocaldb` (installed with Visual Studio's ".NET desktop
  development" or "ASP.NET and web development" workload).

- **Docker** (any OS):
  ```bash
  docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
    -p 1433:1433 --name pos-sql -d mcr.microsoft.com/mssql/server:2022-latest
  ```
  then use a connection string like the one originally in this file
  (`Server=localhost,1433;...;User Id=sa;Password=YourStrong!Passw0rd;...`).

- **A full SQL Server instance already installed**: confirm the SQL Server service is actually
  running, that **TCP/IP is enabled** (SQL Server Configuration Manager → SQL Server Network
  Configuration → Protocols → TCP/IP → Enabled, then restart the service — it's disabled by
  default on Developer/Express installs), that the port (1433 by default) isn't blocked by a
  firewall, and that **SQL Server Authentication (mixed mode)** is enabled if you're connecting
  with `User Id=sa` rather than `Trusted_Connection=True`.

If you just want the app to start without touching a database at all (e.g. to confirm it boots),
set `"ApplyMigrationsOnStartup": false` in `appsettings.json` — but note `/api/auth/login` and
`/api/customers` will still fail without a real database, since they need one.
