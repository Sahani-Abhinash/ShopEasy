using Catalog.Api.Data;
using Catalog.Api.Products;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ShopEasy.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("CatalogDb")
    ?? throw new InvalidOperationException(
        "Connection string 'CatalogDb' is missing. Set it with user-secrets or the ConnectionStrings__CatalogDb environment variable.");

builder.Services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(connectionString));

// Readiness: the service can only serve traffic when its database is reachable
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CatalogDbContext>("catalog-db", tags: [ServiceDefaultsExtensions.ReadyTag]);

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();               // /openapi/v1.json
    app.MapScalarApiReference(options => options.WithTitle("ShopEasy Catalog API")); // /scalar
    app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
}

app.MapProductEndpoints();

app.Run();
