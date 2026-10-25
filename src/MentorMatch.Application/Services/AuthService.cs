using System.Net.Mail;
using MentorMatch.Application.Common;
using MentorMatch.Application.DTOs.Request.Auth;
using MentorMatch.Application.DTOs.Response.Auth;
using MentorMatch.Application.Interfaces.Repositories;
using MentorMatch.Application.Interfaces.Security;
using MentorMatch.Application.Interfaces.Services;
using MentorMatch.Domain.Entities;
using MentorMatch.Domain.Enums;

namespace MentorMatch.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordService passwordService,
    ITokenService tokenService,
    IUnitOfWork unitOfWork) : IAuthService
{
    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var firstName = request.FirstName?.Trim() ?? string.Empty;
        var lastName = request.LastName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName)
            || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
            return Result<AuthResponse>.Failure("Name, email, and password are required.", ErrorType.Validation);

        if (request.Password.Length < 8)
            return Result<AuthResponse>.Failure("Password must be at least 8 characters.", ErrorType.Validation);

        if (firstName.Length > 100 || lastName.Length > 100 || email.Length > 320)
            return Result<AuthResponse>.Failure("A name or email exceeds the allowed length.", ErrorType.Validation);

        if (!MailAddress.TryCreate(email, out var emailAddress) || emailAddress.Address != email)
            return Result<AuthResponse>.Failure("Enter a valid email address.", ErrorType.Validation);

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role)
            || !Enum.IsDefined(role)
            || role == UserRole.Admin)
            return Result<AuthResponse>.Failure("Choose either the Mentor or Mentee role.", ErrorType.Validation);

        if (await userRepository.GetByEmailAsync(email) is not null)
            return Result<AuthResponse>.Failure("An account with this email already exists.", ErrorType.Conflict);

        var user = new User(firstName, lastName, email, "pending", role);
        var passwordHash = passwordService.Hash(user, request.Password);
        user.SetPasswordHash(passwordHash);
        await userRepository.AddAsync(user);
        await unitOfWork.SaveChangesAsync();

        return Result<AuthResponse>.Success(CreateResponse(user));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var user = await userRepository.GetByEmailAsync(email);

        if (user is null || !passwordService.Verify(user, user.PasswordHash, request.Password ?? string.Empty))
            return Result<AuthResponse>.Failure("Email or password is incorrect.", ErrorType.Unauthorized);

        return Result<AuthResponse>.Success(CreateResponse(user));
    }

    private AuthResponse CreateResponse(User user) => new()
    {
        UserId = user.Id,
        Email = user.Email,
        Role = user.Role.ToString(),
        Token = tokenService.CreateToken(user)
    };
}
