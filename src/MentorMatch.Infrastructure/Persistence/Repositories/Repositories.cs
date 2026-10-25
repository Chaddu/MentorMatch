using MentorMatch.Application.Interfaces.Repositories;
using MentorMatch.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MentorMatch.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public Task<User?> GetByIdAsync(int id) => context.Users.FirstOrDefaultAsync(user => user.Id == id);
    public Task<User?> GetByEmailAsync(string email) => context.Users.FirstOrDefaultAsync(user => user.Email == email);
    public async Task AddAsync(User user) => await context.Users.AddAsync(user);
}

public class MentorSkillRepository(AppDbContext context) : IMentorSkillRepository
{
    public Task<MentorSkill?> GetByIdAsync(int id) => context.MentorSkills.FirstOrDefaultAsync(skill => skill.Id == id);

    public Task<List<MentorSkill>> GetByMentorAsync(int mentorId) => context.MentorSkills
        .Where(skill => skill.MentorId == mentorId && skill.IsActive)
        .OrderBy(skill => skill.Name)
        .ToListAsync();

    public async Task AddAsync(MentorSkill skill) => await context.MentorSkills.AddAsync(skill);
}

public class MentorshipRequestRepository(AppDbContext context) : IMentorshipRequestRepository
{
    public Task<MentorshipRequest?> GetByIdAsync(int id) => context.MentorshipRequests.FirstOrDefaultAsync(request => request.Id == id);

    public Task<List<MentorshipRequest>> GetForMenteeAsync(int menteeId) => context.MentorshipRequests
        .Where(request => request.MenteeId == menteeId)
        .OrderByDescending(request => request.CreatedAt)
        .ToListAsync();

    public Task<List<MentorshipRequest>> GetForMentorAsync(int mentorId) => context.MentorshipRequests
        .Where(request => request.MentorId == mentorId)
        .OrderByDescending(request => request.CreatedAt)
        .ToListAsync();

    public async Task AddAsync(MentorshipRequest request) => await context.MentorshipRequests.AddAsync(request);
}
