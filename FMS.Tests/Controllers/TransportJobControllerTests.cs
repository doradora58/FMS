using FMS.Controllers;
using FMS.Core.Models;
using FMS.Core.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FMS.Tests.Controllers;

public class TransportJobControllerTests
{
    [Fact]
    public async Task GivenValidJobWhenCreateJobThenShouldReturnOkResultAsync()
    {
        // Arrange
        var service = Substitute.For<ITransportJobService>();
        var controller = new TransportJobController(service);

        var request = new TransportJobRequest
        {
            OrderId = "JOB-API-01",
            FromAddress = "A",
            ToAddress = "B"
        };

        var dummyJob = new Job { JobId = "JOB-API-01" };
        service.CreateTransportJobAsync(request).Returns(Task.FromResult(dummyJob));

        // Act
        var result = await controller.Create(request);

        // Assert
        await service.Received(1).CreateTransportJobAsync(request);
    }

    [Fact]
    public async Task GivenExistingJobsWhenGetAllThenShouldReturnOkWithJobsAsync()
    {
        // Arrange
        var service = Substitute.For<ITransportJobService>();
        var controller = new TransportJobController(service);

        var dummyJobs = new List<Job>
        {
            new Job { JobId = "JOB-01" },
            new Job { JobId = "JOB-02" }
        };
        service.GetAllJobsAsync().Returns(Task.FromResult<IEnumerable<Job>>(dummyJobs));

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedJobs = Assert.IsAssignableFrom<IEnumerable<Job>>(okResult.Value);
        Assert.Equal(2, returnedJobs.Count());
        await service.Received(1).GetAllJobsAsync();
    }

    [Fact]
    public async Task GivenExistingJobIdWhenGetByIdThenShouldReturnOkWithJobAsync()
    {
        // Arrange
        var service = Substitute.For<ITransportJobService>();
        var controller = new TransportJobController(service);

        var jobId = "JOB-01";
        var dummyJob = new Job { JobId = jobId };
        service.GetJobAsync(jobId).Returns(Task.FromResult<Job?>(dummyJob));

        // Act
        var result = await controller.GetById(jobId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedJob = Assert.IsType<Job>(okResult.Value);
        Assert.Equal(jobId, returnedJob.JobId);
        await service.Received(1).GetJobAsync(jobId);
    }

    [Fact]
    public async Task GivenNonExistingJobIdWhenGetByIdThenShouldReturnNotFoundAsync()
    {
        // Arrange
        var service = Substitute.For<ITransportJobService>();
        var controller = new TransportJobController(service);

        var jobId = "UNKNOWN-ID";
        service.GetJobAsync(jobId).Returns(Task.FromResult<Job?>(null));

        // Act
        var result = await controller.GetById(jobId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        await service.Received(1).GetJobAsync(jobId);
    }
}
