using FMS.Core.Repositories;
using FMS.Core.Services;
using FMS.Infrastructure.Database;
using FMS.Infrastructure.External;
using FMS.Infrastructure.Factories;
using FMS.Infrastructure.Messaging;
using FMS.Infrastructure.Mocks;
using FMS.Infrastructure.VehicleMessaging;
using FMS.JobManagement.Commands;
using FMS.VehicleManagement.Models;
using FMS.VehicleManagement.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace FMS.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IVehicleRegistry, InMemoryVehicleRegistry>();
        services.AddHostedService<Vda5050MqttReceiver>();

        // RabbitMQ
        services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
        services.AddSingleton<IConnection>(sp =>
        {
            var factory = sp.GetRequiredService<IRabbitMqConnectionFactory>();
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
        AddInMemoryJobCreationQueue(services);

        // MongoDB
        services.AddScoped<ITransportJobRepository, MongoTransportJobRepository>();

        // GoogleMap
        services.AddHttpClient<GoogleMapsRoutingService>();
        services.AddSingleton<IRoutingServiceFactory, RoutingServiceFactory>();
        services.AddScoped<IRoutingService, GoogleMapsRoutingService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureMockServices(this IServiceCollection services)
    {
        var vehicleRegistry = new InMemoryVehicleRegistry();
        vehicleRegistry.UpdateConnection("Mock", "AGV-001", "Online", DateTimeOffset.UtcNow);
        vehicleRegistry.UpdateState(new VehicleStateUpdate(
            "Mock",
            "AGV-001",
            new VehiclePosition(35.681236, 139.767125),
            DateTimeOffset.UtcNow));
        services.AddSingleton<IVehicleRegistry>(vehicleRegistry);

        // RabbitMQ
        services.AddSingleton<IMessagePublisher, MockMessagePublisher>();
        AddInMemoryJobCreationQueue(services);
        
        // MongoDB
        services.AddSingleton<ITransportJobRepository, MockTransportJobRepository>();

        // GoogleMap
        services.AddSingleton<IRoutingServiceFactory, MockRoutingServiceFactory>();
        services.AddScoped<IRoutingService, MockRoutingService>();

        return services;
    }

    private static void AddInMemoryJobCreationQueue(IServiceCollection services)
    {
        services.AddSingleton<InMemoryJobCreationQueue>();
        services.AddSingleton<ICommandQueue<CreateTransportJobCommand>>(
            sp => sp.GetRequiredService<InMemoryJobCreationQueue>());
        services.AddHostedService(sp => sp.GetRequiredService<InMemoryJobCreationQueue>());
    }
}
