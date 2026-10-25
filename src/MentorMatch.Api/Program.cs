using System.Security.Claims;
using System.Text;
using MentorMatch.Application.Common;
using MentorMatch.Application.DTOs.Request;
using MentorMatch.Application.DTOs.Request.Auth;
using MentorMatch.Application.DTOs.Request.Skills;
using MentorMatch.Application.Interfaces.Services;
using MentorMatch.Domain.Enums;
using MentorMatch.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Set Jwt:Key to a secret containing at least 32 bytes.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "MentorMatch",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MentorMatch.Api",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => Results.Ok(new { name = "MentorMatch API", status = "running" }));

var auth = app.MapGroup("/api/auth");
auth.MapPost("/register", async (RegisterRequest request, IAuthService service) =>
    ToHttpResult(await service.RegisterAsync(request)));
auth.MapPost("/login", async (LoginRequest request, IAuthService service) =>
    ToHttpResult(await service.LoginAsync(request)));

var skills = app.MapGroup("/api/mentor-skills").RequireAuthorization();
skills.MapPost("/", async (CreateMentorSkillRequest request, ClaimsPrincipal principal, IMentorSkillService service) =>
{
    if (!TryGetUserId(principal, out var userId))
        return Results.Unauthorized();
    return ToHttpResult(await service.CreateAsync(request, userId));
}).RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Mentor)));
skills.MapGet("/mentor/{mentorId:int}", async (int mentorId, IMentorSkillService service) =>
    ToHttpResult(await service.GetByMentorAsync(mentorId)));
skills.MapDelete("/{skillId:int}", async (int skillId, ClaimsPrincipal principal, IMentorSkillService service) =>
{
    if (!TryGetUserId(principal, out var userId))
        return Results.Unauthorized();
    return ToCommandHttpResult(await service.DeactivateAsync(skillId, userId));
});

var requests = app.MapGroup("/api/mentorship-requests").RequireAuthorization();
requests.MapPost("/", async (CreateMentorshipRequest request, ClaimsPrincipal principal, IMentorshipRequestService service) =>
{
    if (!TryGetUserId(principal, out var userId))
        return Results.Unauthorized();
    return ToHttpResult(await service.CreateAsync(request, userId));
}).RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Mentee)));
requests.MapGet("/mine/mentee", async (ClaimsPrincipal principal, IMentorshipRequestService service) =>
{
    if (!TryGetUserId(principal, out var userId))
        return Results.Unauthorized();
    return ToHttpResult(await service.GetForMenteeAsync(userId));
}).RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Mentee)));
requests.MapGet("/mine/mentor", async (ClaimsPrincipal principal, IMentorshipRequestService service) =>
{
    if (!TryGetUserId(principal, out var userId))
        return Results.Unauthorized();
    return ToHttpResult(await service.GetForMentorAsync(userId));
}).RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Mentor)));
requests.MapGet("/{requestId:int}", async (int requestId, ClaimsPrincipal principal, IMentorshipRequestService service) =>
{
    if (!TryGetUserId(principal, out var userId))
        return Results.Unauthorized();
    return ToHttpResult(await service.GetByIdAsync(requestId, userId));
});
requests.MapPost("/{requestId:int}/accept", async (int requestId, ClaimsPrincipal principal, IMentorshipRequestService service) =>
    await ChangeStatus(principal, service, requestId, service.AcceptAsync))
    .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Mentor)));
requests.MapPost("/{requestId:int}/decline", async (int requestId, ClaimsPrincipal principal, IMentorshipRequestService service) =>
    await ChangeStatus(principal, service, requestId, service.DeclineAsync))
    .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Mentor)));
requests.MapPost("/{requestId:int}/cancel", async (int requestId, ClaimsPrincipal principal, IMentorshipRequestService service) =>
    await ChangeStatus(principal, service, requestId, service.CancelAsync));
requests.MapPost("/{requestId:int}/complete", async (int requestId, ClaimsPrincipal principal, IMentorshipRequestService service) =>
    await ChangeStatus(principal, service, requestId, service.CompleteAsync))
    .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Mentor)));

app.Run();

static bool TryGetUserId(ClaimsPrincipal principal, out int userId) =>
    int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

static async Task<IResult> ChangeStatus(
    ClaimsPrincipal principal,
    IMentorshipRequestService service,
    int requestId,
    Func<int, int, Task<Result>> action)
{
    if (!TryGetUserId(principal, out var userId))
        return Results.Unauthorized();
    return ToCommandHttpResult(await action(requestId, userId));
}

static IResult ToHttpResult<T>(Result<T> result) =>
    result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result);

static IResult ToCommandHttpResult(Result result) =>
    result.IsSuccess ? Results.Ok() : ToProblem(result);

static IResult ToProblem(Result result)
{
    var statusCode = result.ErrorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };
    return Results.Problem(statusCode: statusCode, title: result.Error);
}


