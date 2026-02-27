# Training Records & Plans Keeper

## Obviously a work in progrss

![Build Status](https://github.com/CanaanGM/TrainingDB_Tracker_BE/actions/workflows/dotnet.yml/badge.svg)



---

### made this so i can

1. log my workouts
1. see all my plans
1. create new plans
1. keep a record of exercises and how they relate to muscles
1. leep a record of my training equipment

---

## Architecture

This solution is a .NET 8 Web API organized in three main projects:

- API (ASP.NET Core Web API)
  - Controllers for features like authentication, exercises, equipment, muscles, measurements, plans, and sessions
  - Middleware for request logging and global exception handling
  - JWT authentication and rate limiting (strict policy for login)
  - Localization (Accept-Language), Swagger with operation filter for language header
- DataLibrary (EF Core + domain services)
  - `SqliteContext` DbContext (SQLite) with entity configuration and relationships
  - Services encapsulating data access and domain logic (Muscles, Exercises, Equipment, Measurements, Plans, Training Sessions, Users)
  - AutoMapper profile (`DataLibrary/Core/Profiles.cs`) for DTO ↔ entity mapping
- SharedLibrary (DTOs + Helpers)
  - DTOs for read/write shapes
  - Utility helpers (validation, security helpers, pagination primitives)

High-level flow:
- Controllers validate/bind input and call domain services
- Services use EF Core via `SqliteContext` to read/write SQLite DB
- Responses return DTOs or `Result<T>`-based outcomes mapped to HTTP results

### Request Pipeline (API/Program.cs)
- Swagger UI enabled in Development
- Custom middleware:
  - `ExceptionMiddleware` returns ProblemDetails on unhandled exceptions
  - `RequestLoggerMiddleWare` logs request metadata (agent, IP, headers)
- HTTPS redirection, Authorization, Rate Limiter, Request Localization
- Minimal endpoints for smoke checks: `GET /culture`, `GET /helloworld`, `GET /hello?name=…`

### Authentication & Security
- JWT Bearer auth configured via `JwtSettings` in `API/appsettings.json`
- Access tokens are signed (HMAC-SHA512); refresh tokens issued and stored in DB; refresh token written as `HttpOnly` cookie
- `AuthenticatedUserFilter` enforces that the user is authenticated and resolves `userId` from JWT for protected controllers
- Rate limiting: Sliding window policy `strict-login` (5 attempts / 15 minutes) bound to IP, applied to `POST /log-in`

### Localization
- Supported cultures: `en-US`, `ja`, `ar`
- `Accept-Language` header is documented in Swagger via `AcceptLanguageHeaderOperationFilter`

### Database
- SQLite default connection string is set in `DataLibrary/DependencyInjection.cs`
  - Default path: `Data Source = C:\\Users\\Me\\development\\TrainingDB_Tracker_BE\\training_log_v2.db`
  - To change, update the default in `AddDataLibrary` or pass a connection string overload
- Entities configured in `DataLibrary/Context/SqliteContext.cs` with relationships (e.g., many-to-many exercise⇄equipment, exercise⇄trainingType)

### AutoMapper
- Mappings live in `DataLibrary/Core/Profiles.cs` for all entities/DTOs (Exercises, Muscles, Equipment, Plans, Sessions, Users, etc.)

---

## API Routes

All routes below are absolute (controller method attributes start with `/`), base URL defaults to `http://localhost:5134`.

### AuthController (`API/Controllers/AuthController.cs`)
- POST `/register`
  - Body: `UserWriteDto`
  - Creates user, sets refresh token cookie, returns `UserAuthDto`
- POST `/log-in` (Rate limited: `strict-login`)
  - Body: `UserLogInDto`
  - Verifies credentials, sets refresh token cookie, returns `UserAuthDto`

### GenericController (`API/Controllers/GenericController.cs`)
- Muscles
  - GET `/muscles` → `List<MuscleReadDto>`
  - GET `/muscles/search/{searchTerm}` → `List<MuscleReadDto>`
  - GET `/muscles/{groupName}` → `List<MuscleReadDto>` (by muscle group)
  - POST `/muscles/bulk` → Body: `HashSet<MuscleWriteDto>`
- Training Types
  - GET `/types` → `List<TrainingTypeReadDto>`
  - PUT `/types/{typeId}` → Body: `TrainingTypeWriteDto` (update name)
  - POST `/types/bulk` → Body: `HashSet<TrainingTypeWriteDto>`
- Exercises
  - GET `/exercise` → Query: `ExerciseQueryOptions` (filter/sort/page)
    - Returns `PaginatedList<ExerciseReadDto>`
  - GET `/exercise/{name}` → `ExerciseReadDto`
  - GET `/exercise/search/{exercise}` → `List<ExerciseSearchResultDto>`
  - GET `/exercise/csv` → File download `exercise.csv`
  - POST `/exercise` → Body: `ExerciseWriteDto` (create)
  - POST `/exercise/bulk` → Body: `List<ExerciseWriteDto>` (bulk create)
  - PUT `/exercise/{id}` → Body: `ExerciseWriteDto` (update)
  - DELETE `/exercise/{id}` (delete by id)
  - DELETE `/exercise/bulk` → Body: `List<string>` names (bulk delete)
- Equipment
  - GET `/equipment` → `List<EquipmentReadDto>`
  - POST `/equipment` → Body: `EquipmentWriteDto` (upsert by name)
  - POST `/equipment/bulk` → Body: `List<EquipmentWriteDto>`
  - DELETE `/equipment/{equipmentName}`
- Plans
  - POST `/plans` → Body: `TrainingPlanWriteDto` (create)
  - GET `/plans/{id}` → `TrainingPlanReadDto`

### MeasurementsController (`API/Controllers/MeasurementsController.cs`)
- Base route: `/measurements` (Requires auth via `AuthenticatedUserFilter`)
- GET `/measurements` → Authenticated user’s measurements (List)
- GET `/measurements/{id}` → Single measurement
- POST `/measurements` → Body: `MeasurementsWriteDto` (201 Created with new id)
- PUT `/measurements/{measurementId}` → Body: `MeasurementsWriteDto` (204 No Content)
- DELETE `/measurements/{measurementId}` (204 No Content)

### TrainingSessionController (`API/Controllers/TrainingSessionController.cs`)
- Base route: `/training` (Requires auth via `AuthenticatedUserFilter`)
- GET `/training?startDate=&endDate=` → Paginated sessions for current user (page 1, size 10 by default)
- POST `/training/{userId}` → Body: `TrainingSessionWriteDto` (create one)
- POST `/training/bulk` → Body: `List<TrainingSessionWriteDto>` (bulk create)
- PUT `/training/{sessionId}` → Body: `TrainingSessionWriteDto` (update)
- DELETE `/training/{sessionId}` (delete)

Note: Some training endpoints require a `userId` parameter (currently provided via route or action parameter). The authenticated user id is also available through `IUserAccessor`; future refactors may align these to rely solely on JWT.

### Minimal Endpoints (Program.cs)
- GET `/culture` → Current culture display name
- GET `/helloworld` → Localized message from resources
- GET `/hello?name=...` → Localized greeting

---

## Running & Configuration

- Run API: `dotnet watch -p ./API`
- Swagger UI (Development): `http://localhost:5134/swagger`
- JWT settings: `API/appsettings.json` (`Issuer`, `Audience`, `Secret`, `ExpiryInMinutes`)
- Database: default SQLite path is hardcoded in `DataLibrary/DependencyInjection.AddDataLibrary`
  - Update the connection string there or refactor `Program.cs` to pass one from config

## Data Access & Migrations

- EF Core models are scaffolded and mapped in `DataLibrary/Context` and `DataLibrary/Models`
- Example commands (from original notes):

```bash
dotnet ef database update -p .\DataLibrary\ -s .\DataLibrary\ --no-build -c SqliteContext --connection "Data Source = E:\development\c#\TrainingDB_Integration\training_log_v2.db"
dotnet watch -p ./API
```

---

## To throw or to not throw?

The code favors a simple `Result<T>` pattern to carry success/failure and messages from services back to controllers. Exceptions are handled by `ExceptionMiddleware` and returned as ProblemDetails.

## TODO:

- [ ] facilitate more things to to
- [ ] ORM records
- [ ] consider upsert
- [ ] consider upsert plain sql for creation/updating

### dataBase

- the [schema](https://dbdiagram.io/d/workout-tracker-65bf3a4dac844320ae64ab02)
    - on going design. . .

#### Exercises

> Primary Movement - Modifier (if any) - Equipment (if any)
>

```txt

Primary Movement: Bench Press
Modifier: Reverse Grip
Equipment: Barbell (if specified)
Using this structure, you would name the exercise as:

-> Bench Press - Reverse Grip
```

using sqlite, cause i want to use the database in a standalone mobile application that has **no** access to the
internet. _the gym is in its own dimention it seems_.

some tables are normalized some are not, some relate with a juncture table some directly reference their parent. all for
the sake of simplicity, i can always remodel if the need arise.

no stored procedures or json in the database layer.

### DataLibrary project

meant to be a _stand alone layer_ that interacts with the database and handles all things related to it. **it's made so
i can pluck and add to another solution with minimum hassle**, hence it's the way it is.

---

## Commands

- scaffold the database models

```bash
dotnet ef dbcontext scaffold "Data Source=E:/development/c#/TrainingDB_Integration/training_log_v2.db" Microsoft.EntityFrameworkCore.Sqlite --output-dir ModelsV2 -p .\DataLibrary\  --context-dir Context --no-build
```

then copy the `modelsV2` models into `models`.

- migrations

```bash
dotnet ef database update -p .\DataLibrary\ -s .\DataLibrary\ --no-build -c SqliteContext --connection "Data Source = E:\development\c#\TrainingDB_Integration\training_log_v2.db"

```

- Run

```bash
dotnet watch -p ./API
```


- Quick search

```sql
select e.name, e.how_to from exercise e
where e.name like '%plank%'
order by e.name asc
;
```

- For Taking data

```sql
select name, description, how_to,difficulty from exercise;
select name, muscle_group, function, wiki_page_url from muscle;
select name from training_type;
select exercise_id, name, url from exercise_how_to;
select * from exercise_muscle;
select * from exercise_type;
```

## PS

- chat gepteh is good for generation, not for logic, give it the steps and context and it'll fly, tell it to refactor and it'll burn.
