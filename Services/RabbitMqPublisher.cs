using FMS.Models;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace FMS.Services;

public class RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger) : IMessagePublisher
{
    private static readonly ActivitySource _activitySource = new("Fms.Api.Messaging");
    public async Task PublishJobStatusAsync(JobStatusEvent jobEvent)
    {
        using var activity = _activitySource.StartActivity("RabbitMQ.Publish", ActivityKind.Producer);
        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination.name", "job-status-queue");
        activity?.SetTag("messaging.rabbit.destination.routing_key", "job-status-queue");

        // 接続設定（Docker環境では環境変数やappsettingsからホスト名を取得）
        var hostname = configuration["RabbitMQ:HostName"] ?? "localhost";
        var factory = new ConnectionFactory { HostName = hostname };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        // キューの宣言（存在しない場合は作成される）
        await channel.QueueDeclareAsync(
            queue: "job-status-queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var messageJson = JsonSerializer.Serialize(jobEvent);
        var body = Encoding.UTF8.GetBytes(messageJson);

        var props = new BasicProperties();
        props.Headers ??= new Dictionary<string, object?>();

        // 現在のトレースコンテキスト（TraceId / SpanId）をW3Cフォーマットでヘッダーに注入
        if (Activity.Current != null)
        {
            var context = Activity.Current.Context;
            var traceParent = $"00-{context.TraceId}-{context.SpanId}-{(context.TraceFlags.HasFlag(ActivityTraceFlags.Recorded) ? "01" : "00")}";
            props.Headers["traceparent"] = traceParent;
        }
        
        // メッセージのパブリッシュ
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

