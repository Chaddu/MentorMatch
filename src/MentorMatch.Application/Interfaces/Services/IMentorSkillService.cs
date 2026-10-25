using MentorMatch.Application.Common;
using MentorMatch.Application.DTOs.Request.Skills;
using MentorMatch.Application.DTOs.Response.Skills;

namespace MentorMatch.Application.Interfaces.Services;

public interface IMentorSkillService
{
    Task<Result<MentorSkillResponse>> CreateAsync(CreateMentorSkillRequest request, int currentUserId);
    Task<Result<List<MentorSkillResponse>>> GetByMentorAsync(int mentorId);
    Task<Result> DeactivateAsync(int skillId, int currentUserId);
}
