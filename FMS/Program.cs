using FMS.Core.Repositories;
using FMS.Core.Services;
using FMS.Infrastructure.Messaging;
using FMS.Infrastructure.Mocks;
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
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

// Add services to the container.

builder.Services.AddScoped<ITransportJobService, TransportJobService>();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<ITransportJobRepository, MockTransportJobRepository>();
    builder.Services.AddSingleton<IMessagePublisher, MockMessagePublisher>();

    Console.WriteLine("----------------------------------------");
    Console.WriteLine("[Mode] Running with MOCK implementations");
    Console.WriteLine("----------------------------------------");
}
else
{
    builder.Services.AddScoped<ITransportJobRepository, TransportJobRepository>();
    builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

    Console.WriteLine("----------------------------------------");
    Console.WriteLine("[Mode] Running with REAL infrastructure");
    Console.WriteLine("----------------------------------------");
}




builder.Services.AddControllers();

builder.Services.AddSingleton<ITransportJobRepository,TransportJobRepository>();

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

app.UseRouting();
app.MapControllers();
app.MapPrometheusScrapingEndpoint();
app.Run();
