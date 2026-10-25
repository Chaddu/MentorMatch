using MentorMatch.Domain.Entities;

namespace MentorMatch.Application.Interfaces.Repositories;

public interface IMentorshipRequestRepository
{
    Task<MentorshipRequest?> GetByIdAsync(int id);
    Task<List<MentorshipRequest>> GetForMenteeAsync(int menteeId);
    Task<List<MentorshipRequest>> GetForMentorAsync(int mentorId);
    Task AddAsync(MentorshipRequest request);
}
