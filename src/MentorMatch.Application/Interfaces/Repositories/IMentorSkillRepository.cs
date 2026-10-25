using MentorMatch.Domain.Entities;

namespace MentorMatch.Application.Interfaces.Repositories;

public interface IMentorSkillRepository
{
    Task<MentorSkill?> GetByIdAsync(int id);
    Task<List<MentorSkill>> GetByMentorAsync(int mentorId);
    Task AddAsync(MentorSkill skill);
}
