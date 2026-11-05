using System.Diagnostics;
using FMS.Infrastructure.Messaging;
using FMS.JobManagement.Commands;
using FMS.JobManagement.Telemetry;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FMS.Tests.Infrastructure;

public class InMemoryJobCreationQueueTests
{
    [Fact]
    public async Task GivenQueuedCommandWithRequestTraceWhenProcessedThenContinuesTraceAndTagsJob()
    {
        // Arrange
        var processedActivity = new TaskCompletionSource<(ActivityTraceId TraceId, ActivitySpanId ParentSpanId)>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var processedActivityStarted = new TaskCompletionSource<Activity>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == JobActivity.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            SampleUsingParentId = (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity =>
            {
                if (activity.OperationName == "job.create.process")
                {
                    processedActivityStarted.TrySetResult(activity);
                }
            }
        };
        ActivitySource.AddActivityListener(listener);

        var executor = Substitute.For<ICommandExecutor>();
        executor.ExecuteAsync<CreateTransportJobCommand, Unit>(
                Arg.Any<CreateTransportJobCommand>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var current = Activity.Current!;
                processedActivity.TrySetResult((current.TraceId, current.ParentSpanId));
                return Task.FromResult(Unit.Value);
            });

        using var queue = new InMemoryJobCreationQueue(
            executor,
            Substitute.For<ILogger<InMemoryJobCreationQueue>>());
        var parent = new Activity("http.request").SetIdFormat(ActivityIdFormat.W3C).Start();
        var parentTraceId = parent!.TraceId;
        var parentSpanId = parent.SpanId;
        var command = new CreateTransportJobCommand("JOB-123", "ORDER-456", "A", "B")
        {
            TraceParent = parent.Id,
            TraceState = parent.TraceStateString
        };
        parent.Stop();
        await queue.StartAsync(CancellationToken.None);

        // Act
        await queue.EnqueueAsync(command);
        var processedContext = await processedActivity.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var processActivity = await processedActivityStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await queue.StopAsync(CancellationToken.None);

        // Assert
        Assert.Equal(parentTraceId, processedContext.TraceId);
        Assert.Equal(parentSpanId, processedContext.ParentSpanId);
        Assert.Equal(command.JobId, processActivity.GetTagItem("job.id"));
        await executor.Received(1).ExecuteAsync<CreateTransportJobCommand, Unit>(
            command,
            Arg.Any<CancellationToken>());
    }
}
