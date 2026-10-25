namespace MentorMatch.Domain.Common;

public abstract class BaseEntity<TId>
{
    public TId Id { get; protected set; } = default!;
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    public int? CreatedById { get; protected set; }
    public int? UpdatedById { get; protected set; }
    public int? DeletedById { get; protected set; }

    public bool IsDeleted => DeletedAt.HasValue;

    protected void MarkAsUpdated(int? updatedById = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    public void Delete(int? deletedById = null)
    {
        if (IsDeleted)
            throw new DomainException("Entity is already deleted.");

        DeletedAt = DateTime.UtcNow;
        DeletedById = deletedById;
    }

    public void Restore()
    {
        if (!IsDeleted)
            throw new DomainException("Entity is not deleted.");

        DeletedAt = null;
        DeletedById = null;
    }
}
