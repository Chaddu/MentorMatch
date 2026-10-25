using MentorMatch.Application.Common;
using MentorMatch.Application.DTOs.Request;
using MentorMatch.Application.DTOs.Response;
using MentorMatch.Application.Interfaces.Repositories;
using MentorMatch.Application.Interfaces.Services;
using MentorMatch.Domain.Common;
using MentorMatch.Domain.Entities;
using MentorMatch.Domain.Enums;

namespace MentorMatch.Application.Services;

public class MentorshipRequestService(
    IMentorshipRequestRepository requestRepository,
    IUserRepository userRepository,
    IMentorSkillRepository mentorSkillRepository,
    IUnitOfWork unitOfWork) : IMentorshipRequestService
{
    public async Task<Result<MentorshipRequestResponse>> CreateAsync(
        CreateMentorshipRequest request,
        int currentUserId)
    {
        var mentee = await userRepository.GetByIdAsync(currentUserId);
        if (mentee is null)
            return Result<MentorshipRequestResponse>.Failure("User not found.", ErrorType.NotFound);

        if (mentee.Role is not (UserRole.Mentee or UserRole.Admin))
            return Result<MentorshipRequestResponse>.Failure("Only mentees can request a session.", ErrorType.Forbidden);

        var mentor = await userRepository.GetByIdAsync(request.MentorId);
        if (mentor is null || mentor.Role is not (UserRole.Mentor or UserRole.Admin))
            return Result<MentorshipRequestResponse>.Failure("Mentor not found.", ErrorType.NotFound);

        var skill = await mentorSkillRepository.GetByIdAsync(request.SkillId);
        if (skill is null || !skill.IsActive || skill.MentorId != mentor.Id)
            return Result<MentorshipRequestResponse>.Failure("Active skill offer not found for this mentor.", ErrorType.NotFound);

        try
        {
            var mentorshipRequest = new MentorshipRequest(
                mentor.Id,
                mentee.Id,
                skill.Id,
                request.RequestedStartAt,
                request.DurationInMinutes,
                request.Message);

            await requestRepository.AddAsync(mentorshipRequest);
            await unitOfWork.SaveChangesAsync();

            return Result<MentorshipRequestResponse>.Success(Map(mentorshipRequest));
        }
        catch (DomainException exception)
        {
            return Result<MentorshipRequestResponse>.Failure(exception.Message, ErrorType.Validation);
        }
    }

    public async Task<Result<MentorshipRequestResponse>> GetByIdAsync(int requestId, int currentUserId)
    {
        var mentorshipRequest = await requestRepository.GetByIdAsync(requestId);
        if (mentorshipRequest is null)
            return Result<MentorshipRequestResponse>.Failure("Mentorship request not found.", ErrorType.NotFound);

        if (mentorshipRequest.MentorId != currentUserId && mentorshipRequest.MenteeId != currentUserId)
            return Result<MentorshipRequestResponse>.Failure("You cannot view this mentorship request.", ErrorType.Forbidden);

        return Result<MentorshipRequestResponse>.Success(Map(mentorshipRequest));
    }

    public async Task<Result<List<MentorshipRequestResponse>>> GetForMenteeAsync(int currentUserId)
    {
        var requests = await requestRepository.GetForMenteeAsync(currentUserId);
        return Result<List<MentorshipRequestResponse>>.Success(requests.Select(Map).ToList());
    }

    public async Task<Result<List<MentorshipRequestResponse>>> GetForMentorAsync(int currentUserId)
    {
        var requests = await requestRepository.GetForMentorAsync(currentUserId);
        return Result<List<MentorshipRequestResponse>>.Success(requests.Select(Map).ToList());
    }

    public Task<Result> AcceptAsync(int requestId, int currentUserId) =>
        UpdateStatusAsync(requestId, currentUserId, request => request.Accept(currentUserId));

    public Task<Result> DeclineAsync(int requestId, int currentUserId) =>
        UpdateStatusAsync(requestId, currentUserId, request => request.Decline(currentUserId));

    public Task<Result> CancelAsync(int requestId, int currentUserId) =>
        UpdateStatusAsync(requestId, currentUserId, request => request.Cancel(currentUserId));

    public Task<Result> CompleteAsync(int requestId, int currentUserId) =>
        UpdateStatusAsync(requestId, currentUserId, request => request.Complete(currentUserId));

    private async Task<Result> UpdateStatusAsync(
        int requestId,
        int currentUserId,
        Action<MentorshipRequest> update)
    {
        var mentorshipRequest = await requestRepository.GetByIdAsync(requestId);
        if (mentorshipRequest is null)
            return Result.Failure("Mentorship request not found.", ErrorType.NotFound);

        try
        {
            update(mentorshipRequest);
            await unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        catch (DomainException exception)
        {
            var errorType = exception.Message.StartsWith("Only the mentor", StringComparison.Ordinal)
                || exception.Message.StartsWith("Only the mentor or mentee", StringComparison.Ordinal)
                ? ErrorType.Forbidden
                : ErrorType.Conflict;

            return Result.Failure(exception.Message, errorType);
        }
    }

    private static MentorshipRequestResponse Map(MentorshipRequest request) => new()
    {
        Id = request.Id,
        MentorId = request.MentorId,
        MenteeId = request.MenteeId,
        SkillId = request.SkillId,
        RequestedStartAt = request.RequestedStartAt,
        DurationInMinutes = request.DurationInMinutes,
        Message = request.Message,
        Status = request.Status,
        CreatedAt = request.CreatedAt
    };
}
