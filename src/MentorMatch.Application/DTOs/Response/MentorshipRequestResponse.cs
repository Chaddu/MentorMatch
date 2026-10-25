using MentorMatch.Domain.Enums;

namespace MentorMatch.Application.DTOs.Response;

public class MentorshipRequestResponse
{
    public int Id { get; set; }
    public int MentorId { get; set; }
    public int MenteeId { get; set; }
    public int SkillId { get; set; }
    public DateTime RequestedStartAt { get; set; }
    public int DurationInMinutes { get; set; }
    public string Message { get; set; } = string.Empty;
    public MentorshipRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
