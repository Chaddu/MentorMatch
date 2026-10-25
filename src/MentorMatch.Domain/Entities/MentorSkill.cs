using MentorMatch.Domain.Common;

namespace MentorMatch.Domain.Entities;

public class MentorSkill : BaseEntity<int>
{
    public int MentorId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private MentorSkill()
    {
    }

    public MentorSkill(int mentorId, string name, string description)
    {
        if (mentorId <= 0)
            throw new DomainException("Mentor ID must be greater than zero.");

        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedName))
            throw new DomainException("Skill name is required.");

        if (trimmedName.Length > 100)
            throw new DomainException("Skill name cannot exceed 100 characters.");

        MentorId = mentorId;
        Name = trimmedName;
        Description = description?.Trim() ?? string.Empty;
        IsActive = true;
    }

    public void Deactivate(int updatedById)
    {
        if (MentorId != updatedById)
            throw new DomainException("Only the mentor who offers this skill can deactivate it.");

        if (!IsActive)
            throw new DomainException("This skill is already inactive.");

        IsActive = false;
        MarkAsUpdated(updatedById);
    }
}
