using MentorMatch.Application.Common;
using MentorMatch.Application.DTOs.Request.Auth;
using MentorMatch.Application.DTOs.Response.Auth;

namespace MentorMatch.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
}
