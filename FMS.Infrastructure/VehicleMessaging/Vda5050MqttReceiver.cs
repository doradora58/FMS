using System.Text.Json;
using FMS.VehicleManagement.Services;
using FMS.VehicleManagement.Vda5050;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace FMS.Infrastructure.VehicleMessaging;

public sealed class Vda5050MqttReceiver : BackgroundService
{
    private readonly IVehicleRegistry vehicleRegistry;
    private readonly ILogger<Vda5050MqttReceiver> logger;
    private readonly string host;
    private readonly int port;
    private readonly string? username;
    private readonly string? password;
    private readonly bool useTls;
    private readonly string topicRoot;
    private readonly string clientId;

    public Vda5050MqttReceiver(
        IConfiguration configuration,
        IVehicleRegistry vehicleRegistry,
        ILogger<Vda5050MqttReceiver> logger)
    {
        this.vehicleRegistry = vehicleRegistry;
        this.logger = logger;
        host = configuration["Vda5050:Broker:Host"]
            ?? configuration["RabbitMQ:HostName"]
            ?? "localhost";
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new InvalidOperationException("Vda5050:Broker:Host must not be empty.");
        }

        port = ParsePort(configuration["Vda5050:Broker:Port"]);
        username = configuration["Vda5050:Broker:Username"];
        password = configuration["Vda5050:Broker:Password"];
        var configuredTls = configuration["Vda5050:Broker:UseTls"];
        if (!bool.TryParse(configuredTls ?? "false", out var parsedTls))
        {
            throw new InvalidOperationException("Vda5050:Broker:UseTls must be 'true' or 'false'.");
        }
        useTls = parsedTls;

        if (string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Vda5050:Broker:Username is required when a password is configured.");
        }

        topicRoot = (configuration["Vda5050:TopicRoot"] ?? "uagv/v2").Trim('/');
        clientId = configuration["Vda5050:Broker:ClientId"] ?? $"fms-vda5050-{Guid.NewGuid():N}";

        if (string.IsNullOrWhiteSpace(topicRoot) ||
            topicRoot.Split('/').Any(segment => string.IsNullOrWhiteSpace(segment) || segment is "+" or "#"))
        {
            throw new InvalidOperationException("Vda5050:TopicRoot must be a non-empty MQTT topic prefix without empty segments or wildcards.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var retryDelay = TimeSpan.FromSeconds(5);
        while (!stoppingToken.IsCancellationRequested)
        {
            using var client = new MqttFactory().CreateMqttClient();
            client.ApplicationMessageReceivedAsync += HandleMessageAsync;
            client.DisconnectedAsync += args =>
            {
                logger.LogWarning(args.Exception, "VDA 5050 MQTT connection to {Host}:{Port} disconnected.", host, port);
                return Task.CompletedTask;
            };

            try
            {
                await client.ConnectAsync(CreateClientOptions(), stoppingToken);
                await client.SubscribeAsync(CreateSubscriptionOptions(), stoppingToken);
                logger.LogInformation(
                    "Subscribed to VDA 5050 topics under {TopicRoot} via MQTT broker {Host}:{Port}.",
                    topicRoot,
                    host,
                    port);

                while (client.IsConnected && !stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "VDA 5050 MQTT connection to {Host}:{Port} failed.", host, port);
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(cancellationToken: CancellationToken.None);
                }
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(retryDelay, stoppingToken);
            }
        }
    }

    private Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var message = args.ApplicationMessage;
        var payload = message.PayloadSegment.AsMemory();
        try
        {
            if (message.Topic.EndsWith("/state", StringComparison.Ordinal))
            {
                var update = Vda5050StateParser.ParseState(
                    message.Topic,
                    payload,
                    DateTimeOffset.UtcNow,
                    topicRoot);
                vehicleRegistry.UpdateState(update);
            }
            else if (message.Topic.EndsWith("/visualization", StringComparison.Ordinal))
            {
                var update = Vda5050StateParser.ParseState(
                    message.Topic.Replace("/visualization", "/state", StringComparison.Ordinal),
                    payload,
                    DateTimeOffset.UtcNow,
                    topicRoot);
                vehicleRegistry.UpdateState(update);
            }
            else if (message.Topic.EndsWith("/connection", StringComparison.Ordinal))
            {
                var (manufacturer, serialNumber, state) = Vda5050StateParser.ParseConnection(
                    message.Topic,
                    payload,
                    topicRoot);
                vehicleRegistry.UpdateConnection(manufacturer, serialNumber, state, DateTimeOffset.UtcNow);
            }
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Ignoring malformed VDA 5050 payload on topic {Topic}.", message.Topic);
        }
        catch (FormatException exception)
        {
            logger.LogWarning(exception, "Ignoring invalid VDA 5050 message on topic {Topic}.", message.Topic);
        }

        return Task.CompletedTask;
    }

    private MqttClientOptions CreateClientOptions()
    {
        var builder = new MqttClientOptionsBuilder()
            .WithClientId(clientId)
            .WithTcpServer(host, port)
            .WithCleanSession();
        if (!string.IsNullOrWhiteSpace(username))
        {
            builder.WithCredentials(username, password ?? string.Empty);
        }

        if (useTls)
        {
            builder.WithTlsOptions(tls => tls.UseTls());
        }

        return builder.Build();
    }

    private MqttClientSubscribeOptions CreateSubscriptionOptions()
    {
        var builder = new MqttClientSubscribeOptionsBuilder();
        foreach (var messageType in new[] { "state", "visualization", "connection" })
        {
            builder.WithTopicFilter(filter => filter
                .WithTopic($"{topicRoot}/+/+/{messageType}")
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce));
        }

        return builder.Build();
    }

    private static int ParsePort(string? configuredPort)
    {
        if (string.IsNullOrWhiteSpace(configuredPort))
        {
            return 1883;
        }

        if (!int.TryParse(configuredPort, out var port) || port is < 1 or > 65535)
        {
            throw new InvalidOperationException("Vda5050:Broker:Port must be a valid TCP port.");
        }

        return port;
    }
}
