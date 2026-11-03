using FMS.Core.Models;
using FMS.Core.Repositories;
using FMS.Infrastructure.Mocks;
using FMS.Core.Services;
using FMS.JobManagement;
using FMS.JobManagement.Commands;
using FMS.JobManagement.Handlers;
using FMS.JobManagement.Models;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace FMS.Tests.JobManagement;

public class CommandHandlerTests
{
    [Fact]
    public async Task GivenValidCreateCommandWhenHandledThenCreatesAndProcessesJobInBackground()
    {
        // Arrange
        var operations = new List<string>();
        var repository = Substitute.For<ITransportJobRepository>();
        var factory = Substitute.For<IJobFactory>();
        var routingService = Substitute.For<IRoutingService>();
        var publisher = Substitute.For<IMessagePublisher>();
        var command = new CreateTransportJobCommand("JOB-123", "ORDER-456", "A", "B");
        var job = new Job
        {
            JobId = command.JobId,
            SourceOrderId = command.OrderId,
            FromAddress = command.FromAddress,
            ToAddress = command.ToAddress,
            Status = JobStatus.Routing.ToString()
        };
        repository.GetAsync(command.JobId).Returns((IJob?)null);
        factory.CreatePendingRoute(command.JobId, command.OrderId, "A", "B").Returns(_ =>
        {
            operations.Add("create");
            return job;
        });
        repository.AddAsync(job).Returns(_ =>
        {
            operations.Add("insert");
            return Task.CompletedTask;
        });
        routingService.GetRouteAsync("A", "B", Arg.Any<CancellationToken>()).Returns(_ =>
        {
            operations.Add("route");
            return Task.FromResult(new RouteInfo(100, 60));
        });
        repository.UpdateAsync(job).Returns(_ =>
        {
            operations.Add("update");
            return Task.CompletedTask;
        });
        publisher.PublishJobStatusAsync(Arg.Any<JobStatusEvent>()).Returns(_ =>
        {
            operations.Add("publish");
            return Task.CompletedTask;
        });
        var handler = new CreateTransportJobCommandHandler(
            repository,
            factory,
            routingService,
            publisher,
            Substitute.For<Microsoft.Extensions.Logging.ILogger<CreateTransportJobCommandHandler>>());

        // Act
        await handler.HandleAsync(command);

        // Assert
        Assert.Equal(100, job.DistanceMeters);
        Assert.Equal(60, job.EstimatedDurationSeconds);
        Assert.Equal(JobStatus.Queued.ToString(), job.Status);
        await repository.Received(1).AddAsync(job);
        await routingService.Received(1).GetRouteAsync("A", "B", Arg.Any<CancellationToken>());
        await repository.Received(1).UpdateAsync(job);
        await publisher.Received(1).PublishJobStatusAsync(Arg.Is<JobStatusEvent>(e =>
            e.JobId == command.JobId && e.Status == JobStatus.Queued.ToString()));
        Assert.Equal(["create", "insert", "route", "update", "publish"], operations);
    }

    [Fact]
    public async Task GivenRouteServiceFailsWhenCreateCommandIsHandledThenSavesFailedJob()
    {
        // Arrange
        var repository = Substitute.For<ITransportJobRepository>();
        var factory = Substitute.For<IJobFactory>();
        var routingService = Substitute.For<IRoutingService>();
        var publisher = Substitute.For<IMessagePublisher>();
        var command = new CreateTransportJobCommand("JOB-123", "ORDER-456", "A", "B");
        var job = new Job
        {
            JobId = command.JobId,
            SourceOrderId = command.OrderId,
            FromAddress = command.FromAddress,
            ToAddress = command.ToAddress,
            Status = JobStatus.Routing.ToString()
        };
        repository.GetAsync(command.JobId).Returns((IJob?)null);
        factory.CreatePendingRoute(command.JobId, command.OrderId, "A", "B").Returns(job);
        routingService.GetRouteAsync("A", "B", Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<RouteInfo>(new InvalidOperationException("Route unavailable.")));
        var handler = new CreateTransportJobCommandHandler(
            repository,
            factory,
            routingService,
            publisher,
            Substitute.For<Microsoft.Extensions.Logging.ILogger<CreateTransportJobCommandHandler>>());

        // Act
        await handler.HandleAsync(command);

        // Assert
        Assert.Equal(JobStatus.Failed.ToString(), job.Status);
        await repository.Received(1).AddAsync(job);
        await repository.Received(1).UpdateAsync(job);
        await publisher.Received(1).PublishJobStatusAsync(Arg.Is<JobStatusEvent>(e =>
            e.JobId == job.JobId && e.Status == JobStatus.Failed.ToString()));
    }

    [Fact]
    public async Task GivenJobAlreadyProcessedWhenCreateCommandIsRedeliveredThenDoesNotProcessAgain()
    {
        // Arrange
        var repository = Substitute.For<ITransportJobRepository>();
        var routingService = Substitute.For<IRoutingService>();
        var publisher = Substitute.For<IMessagePublisher>();
        var command = new CreateTransportJobCommand("JOB-123", "ORDER-456", "A", "B");
        var job = new Job { JobId = command.JobId, Status = JobStatus.Queued.ToString() };
        repository.GetAsync(job.JobId).Returns(job);
        var handler = new CreateTransportJobCommandHandler(
            repository,
            Substitute.For<IJobFactory>(),
            routingService,
            publisher,
            Substitute.For<Microsoft.Extensions.Logging.ILogger<CreateTransportJobCommandHandler>>());

        // Act
        await handler.HandleAsync(command);

        // Assert
        await routingService.DidNotReceive().GetRouteAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        await repository.DidNotReceive().UpdateAsync(Arg.Any<IJob>());
        await publisher.DidNotReceive().PublishJobStatusAsync(Arg.Any<JobStatusEvent>());
    }

    [Fact]
    public async Task GivenQueuedJobWhenTransitionCommandIsHandledThenSavesAndPublishesAssignedStatus()
    {
        // Arrange
        var operations = new List<string>();
        var repository = Substitute.For<ITransportJobRepository>();
        var publisher = Substitute.For<IMessagePublisher>();
        repository.UpdateAsync(Arg.Any<IJob>()).Returns(_ =>
        {
            operations.Add("save");
            return Task.CompletedTask;
        });
        publisher.PublishJobStatusAsync(Arg.Any<JobStatusEvent>()).Returns(_ =>
        {
            operations.Add("publish");
            return Task.CompletedTask;
        });
        var job = new Job
        {
            JobId = "JOB-123",
            SourceOrderId = "ORDER-456",
            Status = JobStatus.Queued.ToString()
        };
        repository.GetAsync(job.JobId).Returns(job);
        var handler = new TransitionJobStatusCommandHandler(repository, publisher);
        var command = new TransitionJobStatusCommand(job.JobId, JobTrigger.AssignAgv);

        // Act
        await handler.HandleAsync(command);

        // Assert
        await repository.Received(1).UpdateAsync(job);
        Assert.Equal(JobStatus.Assigned.ToString(), job.Status);
        await publisher.Received(1).PublishJobStatusAsync(Arg.Is<JobStatusEvent>(e =>
            e.JobId == job.JobId &&
            e.OrderId == job.SourceOrderId &&
            e.Status == JobStatus.Assigned.ToString()));
        Assert.Equal(["save", "publish"], operations);
    }

    [Fact]
    public async Task GivenLegacyMovingJobWhenTransitionCommandIsHandledThenCompletesJob()
    {
        // Arrange
        var repository = Substitute.For<ITransportJobRepository>();
        var publisher = Substitute.For<IMessagePublisher>();
        var job = new Job
        {
            JobId = "JOB-123",
            SourceOrderId = "ORDER-456",
            Status = "Moving"
        };
        repository.GetAsync(job.JobId).Returns(job);
        var handler = new TransitionJobStatusCommandHandler(repository, publisher);
        var command = new TransitionJobStatusCommand(job.JobId, JobTrigger.Complete);

        // Act
        await handler.HandleAsync(command);

        // Assert
        Assert.Equal(JobStatus.Completed.ToString(), job.Status);
        await repository.Received(1).UpdateAsync(job);
        await publisher.Received(1).PublishJobStatusAsync(Arg.Is<JobStatusEvent>(e =>
            e.Status == JobStatus.Completed.ToString()));
    }

    [Fact]
    public async Task GivenRegisteredHandlerWhenCommandIsExecutedThenDispatchesToHandler()
    {
        // Arrange
        var handler = Substitute.For<ICommandHandler<CreateTransportJobCommand, Unit>>();
        var command = new CreateTransportJobCommand("JOB-123", "ORDER-456", "A", "B");
        using var services = new ServiceCollection()
            .AddSingleton(handler)
            .BuildServiceProvider();
        var executor = new CommandExecutor(services, services.GetRequiredService<IServiceScopeFactory>());

        // Act
        var result = await executor.ExecuteAsync<CreateTransportJobCommand, Unit>(command);

        // Assert
        Assert.Equal(Unit.Value, result);
        await handler.Received(1).HandleAsync(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenRegisteredCommandQueueWhenCommandIsEnqueuedThenDispatchesToQueue()
    {
        // Arrange
        var queue = Substitute.For<ICommandQueue<CreateTransportJobCommand>>();
        var command = new CreateTransportJobCommand("JOB-123", "ORDER-456", "A", "B");
        using var services = new ServiceCollection()
            .AddSingleton(queue)
            .BuildServiceProvider();
        var executor = new CommandExecutor(services, services.GetRequiredService<IServiceScopeFactory>());

        // Act
        await executor.EnqueueAsync(command);

        // Assert
        await queue.Received(1).EnqueueAsync(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenExistingJobWhenMockRepositoryUpdatesAsyncThenJobStatusIsUpdated()
    {
        // Arrange
        var repository = new MockTransportJobRepository();
        var job = new Job { JobId = "JOB-123", Status = JobStatus.Queued.ToString() };
        await repository.AddAsync(job);
        job.Status = JobStatus.Assigned.ToString();

        // Act
        await repository.UpdateAsync(job);

        // Assert
        Assert.Equal(JobStatus.Assigned.ToString(), (await repository.GetAsync(job.JobId))?.Status);
    }
}
