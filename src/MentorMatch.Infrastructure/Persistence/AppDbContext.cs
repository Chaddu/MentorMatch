using MentorMatch.Application.Interfaces.Repositories;
using MentorMatch.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MentorMatch.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<MentorSkill> MentorSkills => Set<MentorSkill>();
    public DbSet<MentorshipRequest> MentorshipRequests => Set<MentorshipRequest>();

    public Task<int> SaveChangesAsync() => base.SaveChangesAsync();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.LastName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(320).IsRequired();
            entity.HasIndex(user => user.Email).IsUnique().HasFilter("[DeletedAt] IS NULL");
            entity.Property(user => user.PasswordHash).IsRequired();
            entity.HasQueryFilter(user => user.DeletedAt == null);
        });

        modelBuilder.Entity<MentorSkill>(entity =>
        {
            entity.HasKey(skill => skill.Id);
            entity.Property(skill => skill.Name).HasMaxLength(100).IsRequired();
            entity.Property(skill => skill.Description).HasMaxLength(1000);
            entity.HasIndex(skill => new { skill.MentorId, skill.Name }).IsUnique().HasFilter("[DeletedAt] IS NULL");
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(skill => skill.MentorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(skill => skill.DeletedAt == null);
        });

        modelBuilder.Entity<MentorshipRequest>(entity =>
        {
            entity.HasKey(request => request.Id);
            entity.Property(request => request.Message).HasMaxLength(1000).IsRequired();
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(request => request.MentorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(request => request.MenteeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<MentorSkill>()
                .WithMany()
                .HasForeignKey(request => request.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(request => request.DeletedAt == null);
        });
    }
}

