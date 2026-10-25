using MentorMatch.Domain.Entities;

namespace MentorMatch.Application.Interfaces.Security;

public interface ITokenService
{
    string CreateToken(User user);
}
