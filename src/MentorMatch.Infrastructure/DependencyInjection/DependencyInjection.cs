using MentorMatch.Application.Interfaces.Repositories;
using MentorMatch.Application.Interfaces.Security;
using MentorMatch.Application.Interfaces.Services;
using MentorMatch.Application.Services;
using MentorMatch.Infrastructure.Persistence;
using MentorMatch.Infrastructure.Persistence.Repositories;
using MentorMatch.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MentorMatch.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMentorSkillRepository, MentorSkillRepository>();
        services.AddScoped<IMentorshipRequestRepository, MentorshipRequestRepository>();
        services.AddScoped<IMentorshipRequestService, MentorshipRequestService>();
        services.AddScoped<IMentorSkillService, MentorSkillService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}

