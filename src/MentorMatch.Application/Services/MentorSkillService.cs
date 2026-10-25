using MentorMatch.Application.Common;
using MentorMatch.Application.DTOs.Request.Skills;
using MentorMatch.Application.DTOs.Response.Skills;
using MentorMatch.Application.Interfaces.Repositories;
using MentorMatch.Application.Interfaces.Services;
using MentorMatch.Domain.Common;
using MentorMatch.Domain.Entities;
using MentorMatch.Domain.Enums;

namespace MentorMatch.Application.Services;

public class MentorSkillService(
    IMentorSkillRepository skillRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IMentorSkillService
{
    public async Task<Result<MentorSkillResponse>> CreateAsync(
        CreateMentorSkillRequest request,
        int currentUserId)
    {
        var mentor = await userRepository.GetByIdAsync(currentUserId);
        if (mentor is null)
            return Result<MentorSkillResponse>.Failure("User not found.", ErrorType.NotFound);

        if (mentor.Role != UserRole.Mentor)
            return Result<MentorSkillResponse>.Failure("Only mentors can offer skills.", ErrorType.Forbidden);

        try
        {
            var skill = new MentorSkill(currentUserId, request.Name, request.Description);
            await skillRepository.AddAsync(skill);
            await unitOfWork.SaveChangesAsync();
            return Result<MentorSkillResponse>.Success(Map(skill));
        }
        catch (DomainException exception)
        {
            return Result<MentorSkillResponse>.Failure(exception.Message, ErrorType.Validation);
        }
    }

    public async Task<Result<List<MentorSkillResponse>>> GetByMentorAsync(int mentorId)
    {
        var skills = await skillRepository.GetByMentorAsync(mentorId);
        return Result<List<MentorSkillResponse>>.Success(skills.Select(Map).ToList());
    }

    public async Task<Result> DeactivateAsync(int skillId, int currentUserId)
    {
        var skill = await skillRepository.GetByIdAsync(skillId);
        if (skill is null)
            return Result.Failure("Skill offer not found.", ErrorType.NotFound);

        try
        {
            skill.Deactivate(currentUserId);
            await unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        catch (DomainException exception)
        {
            var errorType = exception.Message.StartsWith("Only the mentor", StringComparison.Ordinal)
                ? ErrorType.Forbidden
                : ErrorType.Conflict;
            return Result.Failure(exception.Message, errorType);
        }
    }

    private static MentorSkillResponse Map(MentorSkill skill) => new()
    {
        Id = skill.Id,
        MentorId = skill.MentorId,
        Name = skill.Name,
        Description = skill.Description,
        IsActive = skill.IsActive,
        CreatedAt = skill.CreatedAt
    };
}
