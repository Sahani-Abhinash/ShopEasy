using Orders.Api.Customers;
using Orders.Api.Orders;
using Orders.Application.Orders;
using Orders.Infrastructure;
using Orders.Infrastructure.Persistence;
using Scalar.AspNetCore;
using ShopEasy.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOrdersInfrastructure(builder.Configuration);
builder.Services.AddScoped<PlaceOrderHandler>();
builder.Services.AddSingleton(TimeProvider.System);

// Development-only identity until real authentication (Sprint 12)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentCustomer, DevelopmentCurrentCustomer>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrdersDbContext>("orders-db", tags: [ServiceDefaultsExtensions.ReadyTag]);

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("ShopEasy Orders API"));
    app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
}

app.MapOrderEndpoints();

app.Run();
