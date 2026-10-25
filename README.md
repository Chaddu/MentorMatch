# MentorMatch

A layered ASP.NET Core API for finding mentors, offering skills, and scheduling mentorship sessions.

## Projects

- `MentorMatch.Domain` contains entities, lifecycle rules, and domain exceptions.
- `MentorMatch.Application` contains DTOs, service contracts, Result types, and use cases.
- `MentorMatch.Infrastructure` contains EF Core, SQL Server persistence, repositories, password hashing, and token creation.
- `MentorMatch.Api` exposes the HTTP endpoints and JWT authentication.

## Local setup

1. Open `MentorMatch.slnx` in Visual Studio.
2. Restore NuGet packages.
3. Set a local JWT signing key with User Secrets. Use a unique random value at least 32 bytes long:

   ```powershell
   dotnet user-secrets init --project src/MentorMatch.Api
   dotnet user-secrets set "Jwt:Key" "replace-with-a-unique-random-secret-at-least-32-bytes" --project src/MentorMatch.Api
   ```

4. Confirm SQL Server LocalDB is available, or change `ConnectionStrings:DefaultConnection` in local configuration to your SQL Server instance.
5. Apply the initial migration from the solution directory:

   ```powershell
   dotnet ef database update --project src/MentorMatch.Infrastructure --startup-project src/MentorMatch.Api
   ```

6. Run `MentorMatch.Api`.

## First workflow

1. `POST /api/auth/register` with a `Mentor` role.
2. Use the returned bearer token to `POST /api/mentor-skills` and create a skill offer.
3. Register another user with a `Mentee` role.
4. Use the mentee token to create a request at `POST /api/mentorship-requests`.
5. Use the mentor token to accept or decline it.

Session dates should be sent in UTC using an ISO 8601 value ending in `Z`.

## Endpoints

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/mentor-skills` (mentor)
- `GET /api/mentor-skills/mentor/{mentorId}` (authenticated)
- `DELETE /api/mentor-skills/{skillId}` (owner mentor)
- `POST /api/mentorship-requests` (mentee)
- `GET /api/mentorship-requests/mine/mentee` (mentee)
- `GET /api/mentorship-requests/mine/mentor` (mentor)
- `GET /api/mentorship-requests/{requestId}` (participant)
- `POST /api/mentorship-requests/{requestId}/accept` (mentor)
- `POST /api/mentorship-requests/{requestId}/decline` (mentor)
- `POST /api/mentorship-requests/{requestId}/cancel` (participant)
- `POST /api/mentorship-requests/{requestId}/complete` (mentor, after scheduled end)
