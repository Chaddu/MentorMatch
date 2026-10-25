using MentorMatch.Application.Interfaces.Security;
using MentorMatch.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MentorMatch.Infrastructure.Security;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> passwordHasher = new();

    public string Hash(User user, string password) =>
        passwordHasher.HashPassword(user, password);

    public bool Verify(User user, string passwordHash, string password) =>
        passwordHasher.VerifyHashedPassword(user, passwordHash, password)
            != PasswordVerificationResult.Failed;
}
