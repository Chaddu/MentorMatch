using MentorMatch.Domain.Common;
using MentorMatch.Domain.Enums;

namespace MentorMatch.Domain.Entities;

public class MentorshipRequest : BaseEntity<int>
{
    public int MentorId { get; private set; }
    public int MenteeId { get; private set; }
    public int SkillId { get; private set; }
    public DateTime RequestedStartAt { get; private set; }
    public int DurationInMinutes { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public MentorshipRequestStatus Status { get; private set; }

    private MentorshipRequest()
    {
    }

    public MentorshipRequest(
        int mentorId,
        int menteeId,
        int skillId,
        DateTime requestedStartAt,
        int durationInMinutes,
        string message)
    {
        if (mentorId <= 0)
            throw new DomainException("Mentor ID must be greater than zero.");

        if (menteeId <= 0)
            throw new DomainException("Mentee ID must be greater than zero.");

        if (mentorId == menteeId)
            throw new DomainException("A user cannot request mentorship from themselves.");

        if (skillId <= 0)
            throw new DomainException("Skill ID must be greater than zero.");

        if (requestedStartAt <= DateTime.UtcNow)
            throw new DomainException("The requested session must start in the future.");

        if (durationInMinutes is < 15 or > 180)
            throw new DomainException("Session duration must be between 15 and 180 minutes.");

        var trimmedMessage = message?.Trim() ?? string.Empty;
        if (trimmedMessage.Length > 1000)
            throw new DomainException("Message cannot exceed 1000 characters.");

        MentorId = mentorId;
        MenteeId = menteeId;
        SkillId = skillId;
        RequestedStartAt = requestedStartAt;
        DurationInMinutes = durationInMinutes;
        Message = trimmedMessage;
        Status = MentorshipRequestStatus.Pending;
    }

    public void Accept(int mentorId)
    {
        EnsurePending();
        EnsureMentor(mentorId);
        Status = MentorshipRequestStatus.Accepted;
        MarkAsUpdated(mentorId);
    }

    public void Decline(int mentorId)
    {
        EnsurePending();
        EnsureMentor(mentorId);
        Status = MentorshipRequestStatus.Declined;
        MarkAsUpdated(mentorId);
    }

    public void Cancel(int userId)
    {
        if (userId != MentorId && userId != MenteeId)
            throw new DomainException("Only the mentor or mentee can cancel this request.");

        if (Status is not (MentorshipRequestStatus.Pending or MentorshipRequestStatus.Accepted))
            throw new DomainException("Only pending or accepted requests can be cancelled.");

        Status = MentorshipRequestStatus.Cancelled;
        MarkAsUpdated(userId);
    }

    public void Complete(int mentorId)
    {
        EnsureMentor(mentorId);

        if (Status != MentorshipRequestStatus.Accepted)
            throw new DomainException("Only accepted requests can be completed.");

        if (DateTime.UtcNow < RequestedStartAt.AddMinutes(DurationInMinutes))
            throw new DomainException("A session cannot be completed before its scheduled end time.");

        Status = MentorshipRequestStatus.Completed;
        MarkAsUpdated(mentorId);
    }

    private void EnsurePending()
    {
        if (Status != MentorshipRequestStatus.Pending)
            throw new DomainException("Only pending requests can be accepted or declined.");
    }

    private void EnsureMentor(int userId)
    {
        if (userId != MentorId)
            throw new DomainException("Only the mentor can perform this action.");
    }
}
