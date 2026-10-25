using MentorMatch.Domain.Entities;

namespace MentorMatch.Application.Interfaces.Security;

public interface IPasswordService
{
    string Hash(User user, string password);
    bool Verify(User user, string passwordHash, string password);
}
