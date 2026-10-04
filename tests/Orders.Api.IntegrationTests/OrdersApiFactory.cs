using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Catalog;
using Orders.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Orders.Api.IntegrationTests;

/// <summary>Orders API in memory + real PostgreSQL in Docker + fake Catalog.</summary>
public sealed class OrdersApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17").Build();

    public FakeCatalogClient Catalog { get; } = new();

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync(TestContext.Current.CancellationToken);

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        await db.Database.MigrateAsync(TestContext.Current.CancellationToken);
    }

    public async Task<int> CountOrdersWithKeyAsync(Guid customerId, string idempotencyKey)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        return await db.Orders.CountAsync(
            o => o.CustomerId == customerId && EF.Property<string>(o, "IdempotencyKey") == idempotencyKey,
            TestContext.Current.CancellationToken);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:OrdersDb", _database.GetConnectionString());
        builder.UseSetting("Services:Catalog", "http://catalog.invalid/");

        builder.ConfigureTestServices(services => services.AddSingleton<ICatalogClient>(Catalog));
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class OrdersApiTestGroup : ICollectionFixture<OrdersApiFactory>
{
    public const string Name = "Orders API";
}
