using FMS.Controllers;
using FMS.Core.Models;
using FMS.Facade;
using FMS.JobManagement.Models;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FMS.Tests.Controllers;

public class TransportJobControllerTests
{
    [Fact]
    public async Task GivenValidJobWhenCreateJobThenShouldReturnOkResultAsync()
    {
        // Arrange
        var facade = Substitute.For<ITransportJobFacade>();
        var controller = new TransportJobController(facade);

        var request = new TransportJobRequest
        {
            OrderId = "JOB-API-01",
            FromAddress = "A",
            ToAddress = "B"
        };

        var dummyJob = new Job { JobId = "JOB-API-01" };
        facade.CreateTransportJobAsync(request).Returns(Task.FromResult<IJob>(dummyJob)); // 👈 Task.FromResult<IJob> に修正

        // Act
        var result = await controller.Create(request);

        // Assert
        await facade.Received(1).CreateTransportJobAsync(request);
    }

    [Fact]
    public async Task GivenExistingJobsWhenGetAllThenShouldReturnOkWithJobsAsync()
    {
        // Arrange
        var facade = Substitute.For<ITransportJobFacade>();
        var controller = new TransportJobController(facade);

        var dummyJobs = new List<Job>
        {
            new Job { JobId = "JOB-01" },
            new Job { JobId = "JOB-02" }
        };
        facade.GetAllJobsAsync().Returns(Task.FromResult<IEnumerable<IJob>>(dummyJobs));

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedJobs = Assert.IsAssignableFrom<IEnumerable<Job>>(okResult.Value);
        Assert.Equal(2, returnedJobs.Count());
        await facade.Received(1).GetAllJobsAsync();
    }

    [Fact]
    public async Task GivenExistingJobIdWhenGetByIdThenShouldReturnOkWithJobAsync()
    {
        // Arrange
        var facade = Substitute.For<ITransportJobFacade>();
        var controller = new TransportJobController(facade);

        var jobId = "JOB-01";
        var dummyJob = new Job { JobId = jobId };
        facade.GetJobAsync(jobId).Returns(Task.FromResult<IJob?>(dummyJob));

        // Act
        var result = await controller.GetById(jobId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedJob = Assert.IsType<Job>(okResult.Value);
        Assert.Equal(jobId, returnedJob.JobId);
        await facade.Received(1).GetJobAsync(jobId);
    }

    [Fact]
    public async Task GivenNonExistingJobIdWhenGetByIdThenShouldReturnNotFoundAsync()
    {
        // Arrange
        var facade = Substitute.For<ITransportJobFacade>();
        var controller = new TransportJobController(facade);

        var jobId = "UNKNOWN-ID";
        facade.GetJobAsync(jobId).Returns(Task.FromResult<IJob?>(null));

        // Act
        var result = await controller.GetById(jobId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        await facade.Received(1).GetJobAsync(jobId);
    }
}