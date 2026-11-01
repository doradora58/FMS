using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Infrastructure.Messaging;

public class RabbitMqConnectionFactory(IConfiguration configuration, ILogger<RabbitMqConnectionFactory> logger) : IRabbitMqConnectionFactory
{
    public async Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var hostname = configuration["RabbitMQ:HostName"] ?? "localhost";
        var factory = new ConnectionFactory { HostName = hostname };

        int maxRetries = 5;
        int delaySeconds = 3;

        for (int i = 1; i <= maxRetries; i++)
        {
            try
            {
                logger.LogInformation("Connecting to RabbitMQ at {HostName} (Attempt {Attempt}/{Max})...", hostname, i, maxRetries);
                var connection = await factory.CreateConnectionAsync(cancellationToken);
                logger.LogInformation("Successfully connected to RabbitMQ.");
                return connection;
            }
            catch (Exception ex) when (i < maxRetries)
            {
                logger.LogWarning(ex, "Failed to connect to RabbitMQ. Retrying in {Seconds}s...", delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
            }
        }
        return await factory.CreateConnectionAsync(cancellationToken);
    }
}
