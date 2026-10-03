using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace ShopEasy.ServiceDefaults;

/// <summary>
/// Cross-cutting setup shared by every ShopEasy service.
/// Filled in sprint by sprint: health checks and ProblemDetails (Sprint 1),
/// OpenTelemetry (Sprint 7).
/// </summary>
public static class ServiceDefaultsExtensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        return app;
    }
}
