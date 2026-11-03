using FMS.Core.Repositories;
using FMS.Core.Services;
using FMS.Infrastructure.Database;
using FMS.Infrastructure.External;
using FMS.Infrastructure.Factories;
using FMS.Infrastructure.Messaging;
using FMS.Infrastructure.Mocks;
using FMS.JobManagement.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
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
