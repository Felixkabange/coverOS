using CoverOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoverOS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CoverOsDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("CoverOS");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseNpgsql(connectionString);
            }
        });

        services.AddScoped<CoverOS.Application.Authentication.IAccountStore, CoverOS.Infrastructure.Authentication.AccountStore>();
        services.AddSingleton<CoverOS.Application.Authentication.IPasswordService, CoverOS.Infrastructure.Authentication.PasswordService>();
        services.AddScoped<CoverOS.Application.Authentication.AccountService>();
        return services;
    }
}