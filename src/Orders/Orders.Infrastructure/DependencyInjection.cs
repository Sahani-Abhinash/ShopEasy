using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Catalog;
using Orders.Application.Orders;
using Orders.Infrastructure.Catalog;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrdersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("OrdersDb")
            ?? throw new InvalidOperationException(
                "Connection string 'OrdersDb' is missing. Set it with user-secrets or the ConnectionStrings__OrdersDb environment variable.");

        var catalogUrl = configuration["Services:Catalog"]
            ?? throw new InvalidOperationException("Configuration 'Services:Catalog' (Catalog base URL) is missing.");

        services.AddDbContext<OrdersDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddHttpClient<ICatalogClient, CatalogHttpClient>(client => client.BaseAddress = new Uri(catalogUrl))
            .AddStandardResilienceHandler(); // retry, timeout, circuit breaker (Chapter 2, §8)

        return services;
    }
}
