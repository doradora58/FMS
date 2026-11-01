using FMS.Core.Repositories;
using FMS.Infrastructure.Database;
using FMS.Infrastructure.Messaging;
using FMS.Infrastructure.Mocks;
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
        services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
        services.AddSingleton<IConnection>(sp =>
        {
            var factory = sp.GetRequiredService<IRabbitMqConnectionFactory>();
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

        services.AddScoped<ITransportJobRepository, MongoTransportJobRepository>();
        return services;
    }

    public static IServiceCollection AddInfrastructureMockServices(this IServiceCollection services)
    {
        services.AddSingleton<IMessagePublisher, MockMessagePublisher>();
        services.AddSingleton<ITransportJobRepository, MockTransportJobRepository>();
        return services;
    }
}
