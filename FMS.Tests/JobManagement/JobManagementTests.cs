using FMS.Core.Models;
using FMS.Core.Repositories;
using FMS.Infrastructure.Mocks;
using FMS.JobManagement;
using FMS.JobManagement.Models;
using FMS.JobManagement.Services;
using NSubstitute;
using JobManagementService = FMS.JobManagement.Services.JobManagement;

namespace FMS.Tests.JobManagement;

public class JobManagementTests
{
    [Fact]
    public async Task GivenValidJobWhenCreateAndSaveJobAsyncThenPublishesQueuedStatusAfterSaving()
    {
        // Arrange
        var operations = new List<string>();
        var repository = Substitute.For<ITransportJobRepository>();
        var factory = Substitute.For<IJobFactory>();
        var publisher = Substitute.For<IMessagePublisher>();
        repository.AddAsync(Arg.Any<IJob>()).Returns(_ =>
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
        factory.Create("ORDER-456", "A", "B", 100, 60).Returns(job);
        var management = new JobManagementService(repository, factory, publisher);

        // Act
        await management.CreateAndSaveJobAsync("ORDER-456", "A", "B", 100, 60);

        // Assert
        await repository.Received(1).AddAsync(job);
        await publisher.Received(1).PublishJobStatusAsync(Arg.Is<JobStatusEvent>(e =>
            e.JobId == job.JobId &&
            e.OrderId == job.SourceOrderId &&
            e.Status == JobStatus.Queued.ToString()));
        Assert.Equal(["save", "publish"], operations);
    }

    [Fact]
    public async Task GivenQueuedJobWhenTransitionStatusAsyncThenPublishesAssignedStatusAfterUpdating()
    {
        // Arrange
        var operations = new List<string>();
        var repository = Substitute.For<ITransportJobRepository>();
        var factory = Substitute.For<IJobFactory>();
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
        var management = new JobManagementService(repository, factory, publisher);

        // Act
        await management.TransitionStatusAsync(job.JobId, JobTrigger.AssignAgv);

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
    public async Task GivenLegacyMovingJobWhenTransitionStatusAsyncThenCompletesJob()
    {
        // Arrange
        var repository = Substitute.For<ITransportJobRepository>();
        var factory = Substitute.For<IJobFactory>();
        var publisher = Substitute.For<IMessagePublisher>();
        var job = new Job
        {
            JobId = "JOB-123",
            SourceOrderId = "ORDER-456",
            Status = "Moving"
        };
        repository.GetAsync(job.JobId).Returns(job);
        var management = new JobManagementService(repository, factory, publisher);

        // Act
        await management.TransitionStatusAsync(job.JobId, JobTrigger.Complete);

        // Assert
        Assert.Equal(JobStatus.Completed.ToString(), job.Status);
        await repository.Received(1).UpdateAsync(job);
        await publisher.Received(1).PublishJobStatusAsync(Arg.Is<JobStatusEvent>(e =>
            e.Status == JobStatus.Completed.ToString()));
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
