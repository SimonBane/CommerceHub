using CommerceHub.BuildingBlocks;
using CommerceHub.GatewayApi.Configuration;
using CommerceHub.GatewayApi.Extensions;
using CommerceHub.GatewayApi.Features.Me;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Yarp.ReverseProxy.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSwaggerGen();

builder.Services.AddGatewayServices(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.MapMeEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        var config = app.Services.GetRequiredService<IOptionsMonitor<ReverseProxyDocumentFilterConfig>>().CurrentValue;
        foreach (var cluster in config.Clusters)
        {
            options.SwaggerEndpoint($"/swagger/{cluster.Key}/swagger.json", cluster.Key);
        }
    });
}

app.MapDefaultEndpoints();

app.Run();
