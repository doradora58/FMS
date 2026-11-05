using FMS.Core.Models;
using FMS.Core.Repositories;
using FMS.Core.Services;
using FMS.Facade;
using FMS.Infrastructure.Database;
using FMS.Infrastructure.Extensions;
using FMS.Infrastructure.Messaging;
using FMS.Infrastructure.Mocks;
using FMS.JobManagement;
using FMS.JobManagement.Commands;
using FMS.JobManagement.Handlers;
using FMS.JobManagement.Services;
using FMS.JobManagement.Telemetry;
using FMS.VehicleManagement.Services;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry
const string serviceName = "FMS";
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("Fms.Api.Messaging")
        .AddSource(JobActivity.SourceName)
        .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources")
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

// Add services to the container.

builder.Services.AddScoped<IJobFactory, JobFactory>();
builder.Services.AddSingleton<ICommandExecutor, CommandExecutor>();
builder.Services.AddScoped<ICommandHandler<CreateTransportJobCommand, Unit>, CreateTransportJobCommandHandler>();
builder.Services.AddScoped<ICommandHandler<TransitionJobStatusCommand, Unit>, TransitionJobStatusCommandHandler>();
builder.Services.AddSingleton<ICommandValidator<CreateTransportJobCommand>, CreateTransportJobCommandValidator>();
builder.Services.AddScoped<IJobQueries, JobQueries>();
builder.Services.AddScoped<ITransportJobFacade, TransportJobFacade>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddInfrastructureMockServices();
    Console.WriteLine("----------------------------------------");
    Console.WriteLine("[Mode] Running with MOCK implementations");
    Console.WriteLine("----------------------------------------");
}
else
{
    builder.Services.AddInfrastructureServices();
    Console.WriteLine("----------------------------------------");
    Console.WriteLine("[Mode] Running with REAL infrastructure");
    Console.WriteLine("----------------------------------------");
}

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/version", () =>
{
    var version = Environment.GetEnvironmentVariable("APP_VERSION") ?? "dev";
    return Results.Ok(new { version });
});

app.MapGet("/api/maps/config", (IConfiguration configuration) =>
{
    var apiKey = configuration["GoogleMaps:JavaScriptApiKey"];
    return string.IsNullOrWhiteSpace(apiKey)
        ? Results.Ok(new { configured = false })
        : Results.Ok(new { configured = true, apiKey });
});

app.MapGet("/api/vehicles", (IVehicleRegistry vehicleRegistry) =>
    Results.Ok(vehicleRegistry.GetVehicles()));

app.UseRouting();
app.MapControllers();
app.MapPrometheusScrapingEndpoint();
app.Run();
