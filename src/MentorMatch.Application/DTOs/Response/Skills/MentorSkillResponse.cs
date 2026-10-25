namespace MentorMatch.Application.DTOs.Response.Skills;

public class MentorSkillResponse
{
    public int Id { get; set; }
    public int MentorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
