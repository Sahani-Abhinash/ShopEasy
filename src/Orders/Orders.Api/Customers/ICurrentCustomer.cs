namespace Orders.Api.Customers;

/// <summary>Who is calling. Replaced by the token's "sub" claim when authentication arrives (Sprint 12).</summary>
internal interface ICurrentCustomer
{
    Guid Id { get; }
}

/// <summary>
/// TEMPORARY: development identity. Uses the <c>X-Customer-Id</c> header if present (to test
/// "customers see only their own orders"), otherwise <c>Development:CustomerId</c> from configuration.
/// Must be replaced before any real deployment.
/// </summary>
internal sealed class DevelopmentCurrentCustomer(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    : ICurrentCustomer
{
    public const string HeaderName = "X-Customer-Id";

    private static readonly Guid FallbackCustomerId = Guid.Parse("c0ffee00-0000-4000-8000-000000000001");

    public Guid Id
    {
        get
        {
            var header = httpContextAccessor.HttpContext?.Request.Headers[HeaderName].ToString();
            if (Guid.TryParse(header, out var fromHeader))
            {
                return fromHeader;
            }

            return Guid.TryParse(configuration["Development:CustomerId"], out var configured)
                ? configured
                : FallbackCustomerId;
        }
    }
}
