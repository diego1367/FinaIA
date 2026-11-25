using CreditRequests.Application.Interfaces;
using CreditRequests.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CreditRequests.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCreditRequestsInfrastructure(
        this IServiceCollection services,
        string? connectionString)
    {
        services.AddDbContext<CreditRequestsDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<ICreditRequestRepository, EfCreditRequestRepository>();
        return services;
    }
}
