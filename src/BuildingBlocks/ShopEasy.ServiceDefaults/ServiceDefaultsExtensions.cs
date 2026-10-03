using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace ShopEasy.ServiceDefaults;

/// <summary>
/// Cross-cutting setup shared by every ShopEasy service (Chapter 2, §10).
/// OpenTelemetry is added in Sprint 7.
/// </summary>
public static class ServiceDefaultsExtensions
{
    /// <summary>Tag for checks that decide whether the process must be restarted.</summary>
    public const string LiveTag = "live";

    /// <summary>Tag for checks that decide whether the service can receive traffic (e.g. database).</summary>
    public const string ReadyTag = "ready";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        // RFC 9457 error responses for all failures; no stack traces leak to clients
        builder.Services.AddProblemDetails();

        // Liveness only checks the process itself, never dependencies (Chapter 6, §5)
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), [LiveTag]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseExceptionHandler();
        app.UseStatusCodePages();

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(LiveTag),
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(ReadyTag),
        });

        return app;
    }
}
