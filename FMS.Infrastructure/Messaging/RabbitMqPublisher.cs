using FMS.Core.Models;
using FMS.Core.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace FMS.Infrastructure.Messaging;

public class RabbitMqPublisher(IConnection connection, ILogger<RabbitMqPublisher> logger) : IMessagePublisher
{
    private static readonly ActivitySource _activitySource = new("Fms.Api.Messaging"); 
    private const string QueueName = "job-status-queue";
    private static bool _isQueueDeclared = false;
    
    public async Task PublishJobStatusAsync(JobStatusEvent jobEvent)
    {
        using var activity = _activitySource.StartActivity("RabbitMQ.Publish", ActivityKind.Producer);
        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination.name", QueueName);
        activity?.SetTag("messaging.rabbit.destination.routing_key", QueueName);

        using var channel = await connection.CreateChannelAsync();

        if (!_isQueueDeclared)
        {
            await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            _isQueueDeclared = true;
        }

        var messageJson = JsonSerializer.Serialize(jobEvent);
        var body = Encoding.UTF8.GetBytes(messageJson);

        var props = new BasicProperties();
        props.Headers ??= new Dictionary<string, object?>();

        if (Activity.Current != null)
        {
            var context = Activity.Current.Context;
            var traceParent = $"00-{context.TraceId}-{context.SpanId}-{(context.TraceFlags.HasFlag(ActivityTraceFlags.Recorded) ? "01" : "00")}";
            props.Headers["traceparent"] = traceParent;
        }
        
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "job-status-queue",
            mandatory: false,
            basicProperties: props,
            body: body
        );

        logger.LogInformation("Published JobStatusEvent for JobId: {JobId}, Status: {Status}", jobEvent.JobId, jobEvent.Status);
    }

}

