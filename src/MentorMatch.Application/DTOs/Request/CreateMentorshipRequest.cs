namespace MentorMatch.Application.DTOs.Request;

public class CreateMentorshipRequest
{
    public int MentorId { get; set; }
    public int SkillId { get; set; }
    public DateTime RequestedStartAt { get; set; }
    public int DurationInMinutes { get; set; }
    public string Message { get; set; } = string.Empty;
}
