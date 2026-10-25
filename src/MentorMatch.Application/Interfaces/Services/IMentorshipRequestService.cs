using MentorMatch.Application.Common;
using MentorMatch.Application.DTOs.Request;
using MentorMatch.Application.DTOs.Response;

namespace MentorMatch.Application.Interfaces.Services;

public interface IMentorshipRequestService
{
    Task<Result<MentorshipRequestResponse>> CreateAsync(
        CreateMentorshipRequest request,
        int currentUserId);

    Task<Result<MentorshipRequestResponse>> GetByIdAsync(
        int requestId,
        int currentUserId);

    Task<Result<List<MentorshipRequestResponse>>> GetForMenteeAsync(int currentUserId);

    Task<Result<List<MentorshipRequestResponse>>> GetForMentorAsync(int currentUserId);

    Task<Result> AcceptAsync(int requestId, int currentUserId);

    Task<Result> DeclineAsync(int requestId, int currentUserId);

    Task<Result> CancelAsync(int requestId, int currentUserId);

    Task<Result> CompleteAsync(int requestId, int currentUserId);
}
