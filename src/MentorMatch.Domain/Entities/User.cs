using MentorMatch.Domain.Common;
using MentorMatch.Domain.Enums;

namespace MentorMatch.Domain.Entities;

public class User : BaseEntity<int>
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public void SetPasswordHash(string passwordHash) => PasswordHash = passwordHash;

    private User()
    {
    }

    public User(string firstName, string lastName, string email, string passwordHash, UserRole role)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
    }
}

