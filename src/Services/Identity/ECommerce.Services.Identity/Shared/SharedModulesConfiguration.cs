using Asp.Versioning;
using Asp.Versioning.Builder;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Monitoring;
using ECommerce.Services.Identity.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Services.Identity.Shared.Extensions.WebApplicationBuilderExtensions;

namespace ECommerce.Services.Identity.Shared;

public class SharedModulesConfiguration : ISharedModulesConfiguration
{
    public const string IdentityModulePrefixUri = "api/v{version:apiVersion}/identity";

    public WebApplicationBuilder AddSharedModuleServices(WebApplicationBuilder builder)
    {
        builder.AddInfrastructure();
        return builder;
    }

    public async Task<WebApplication> ConfigureSharedModule(WebApplication app)
    {
        if (app.Environment.IsEnvironment("test") == false)
        {
            app.UseMonitoring();
            app.UseIdentityServer();
        }

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

            return $"Identity Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();

        return endpoints;
    }
}
