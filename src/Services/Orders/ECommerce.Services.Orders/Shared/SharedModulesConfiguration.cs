using Asp.Versioning;
using Asp.Versioning.Builder;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Monitoring;
using ECommerce.Services.Orders.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Services.Orders.Shared.Extensions.WebApplicationBuilderExtensions;

namespace ECommerce.Services.Orders.Shared;

public class SharedModulesConfiguration : ISharedModulesConfiguration
{
    public const string OrderModulePrefixUri = "api/v{version:apiVersion}/orders";
    public WebApplicationBuilder AddSharedModuleServices(WebApplicationBuilder builder)
    {
        builder.AddInfrastructure();

        builder.AddStorage();

        return builder;
    }

    public async Task<WebApplication> ConfigureSharedModule(WebApplication app)
    {
        if (app.Environment.IsEnvironment("test") == false)
            app.UseMonitoring();

        await app.ApplyDatabaseMigrations(app.Logger);
        await app.SeedData(app.Logger, app.Environment);

        await app.UseInfrastructure(app.Logger);

        return app;
    }

    public IEndpointRouteBuilder MapSharedModuleEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext context) =>
        {
            var requestId = context.Request.Headers.TryGetValue("X-Request-Id", out var requestIdHeader)
                ? requestIdHeader.FirstOrDefault()
                : string.Empty;

            return $"Orders Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();

        return endpoints;
    }
}
