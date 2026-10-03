using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Catalog.Api.IntegrationTests;

[Collection(CatalogApiTestGroup.Name)]
public sealed class HealthEndpointsTests(CatalogApiFactory factory)
{
    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthEndpointsAreHealthyWhenDatabaseIsReachable(string path)
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReadyFailsButLiveStaysHealthyWhenDatabaseIsUnreachable()
    {
        // Nothing listens on port 1: the database is "down"
        await using var unreachable = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting(
                "ConnectionStrings:CatalogDb",
                "Host=127.0.0.1;Port=1;Database=catalog_db;Username=x;Password=x;Timeout=2");
        });
        using var client = unreachable.CreateClient();

        var ready = await client.GetAsync("/health/ready", TestContext.Current.CancellationToken);
        var live = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, ready.StatusCode);
        Assert.Equal(HttpStatusCode.OK, live.StatusCode);
    }
}
