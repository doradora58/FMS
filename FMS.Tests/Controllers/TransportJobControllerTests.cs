using System.Text.Json;
using FMS.Controllers;
using FMS.Core.Models;
using FMS.Facade;
using FMS.JobManagement.Commands;
using FMS.JobManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FMS.Tests.Controllers;

public class TransportJobControllerTests
{
    [Fact]
    public async Task GivenAcceptedJobWhenCreateJobThenReturnsAcceptedResponse()
    {
        // Arrange
        var facade = Substitute.For<ITransportJobFacade>();
        var executor = Substitute.For<ICommandExecutor>();
        var controller = new TransportJobController(
            facade,
            executor,
            Substitute.For<ILogger<TransportJobController>>());
        var context = new DefaultHttpContext();
        var responseFeature = new CapturingResponseFeature();
        context.Features.Set<IHttpResponseFeature>(responseFeature);
        controller.ControllerContext = new ControllerContext { HttpContext = context };

        var request = new TransportJobRequest
        {
            OrderId = "JOB-API-01",
            FromAddress = "A",
            ToAddress = "B"
        };

        const string jobId = "JOB-API-01";
        facade.CreateTransportJobAsync(request).Returns(Task.FromResult(
            new CreateTransportJobCommand(jobId, request.OrderId, request.FromAddress, request.ToAddress)));

        // Act
        var result = await controller.Create(request);

        // Assert
        var accepted = Assert.IsType<AcceptedAtActionResult>(result);
        Assert.Equal(jobId, accepted.RouteValues?["jobId"]);
        using var response = JsonDocument.Parse(JsonSerializer.Serialize(accepted.Value));
        Assert.Equal("Job accepted for processing", response.RootElement.GetProperty("message").GetString());
        Assert.Equal(jobId, response.RootElement.GetProperty("jobId").GetString());
        Assert.Equal(request.OrderId, response.RootElement.GetProperty("orderId").GetString());
        Assert.Equal("Accepted", response.RootElement.GetProperty("status").GetString());
        await executor.DidNotReceive().EnqueueAsync(
            Arg.Any<CreateTransportJobCommand>(),
            Arg.Any<CancellationToken>());
        await responseFeature.CompleteAsync();
        await executor.Received(1).EnqueueAsync(
            Arg.Is<CreateTransportJobCommand>(command => command.JobId == jobId),
            CancellationToken.None);
        await facade.Received(1).CreateTransportJobAsync(request);
    }

    [Fact]
    public async Task GivenInvalidCreateCommandWhenCreateJobThenReturnsValidationProblem()
    {
        // Arrange
        var facade = Substitute.For<ITransportJobFacade>();
        facade.CreateTransportJobAsync(Arg.Any<TransportJobRequest>())
            .Returns<Task<CreateTransportJobCommand>>(_ => throw new FMS.JobManagement.Commands.CommandValidationException(
                new Dictionary<string, string[]> { ["FromAddress"] = ["FromAddress is required."] }));
        var controller = new TransportJobController(
            facade,
            Substitute.For<ICommandExecutor>(),
            Substitute.For<ILogger<TransportJobController>>());

        // Act
        var result = await controller.Create(new TransportJobRequest());

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<ValidationProblemDetails>(badRequest.Value);
    }

    [Fact]
    public async Task GivenExistingJobsWhenGetAllThenShouldReturnOkWithJobsAsync()
    {
        // Arrange
        var facade = Substitute.For<ITransportJobFacade>();
        var controller = new TransportJobController(
            facade,
            Substitute.For<ICommandExecutor>(),
            Substitute.For<ILogger<TransportJobController>>());

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
        var controller = new TransportJobController(
            facade,
            Substitute.For<ICommandExecutor>(),
            Substitute.For<ILogger<TransportJobController>>());

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
        var controller = new TransportJobController(
            facade,
            Substitute.For<ICommandExecutor>(),
            Substitute.For<ILogger<TransportJobController>>());

        var jobId = "UNKNOWN-ID";
        facade.GetJobAsync(jobId).Returns(Task.FromResult<IJob?>(null));

        // Act
        var result = await controller.GetById(jobId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        await facade.Received(1).GetJobAsync(jobId);
    }

    private sealed class CapturingResponseFeature : IHttpResponseFeature
    {
        private readonly List<(Func<object, Task> Callback, object State)> _completedCallbacks = [];

        public int StatusCode { get; set; }
        public string? ReasonPhrase { get; set; }
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public Stream Body { get; set; } = Stream.Null;
        public bool HasStarted { get; private set; }

        public void OnStarting(Func<object, Task> callback, object state)
        {
        }

        public void OnCompleted(Func<object, Task> callback, object state)
        {
            _completedCallbacks.Add((callback, state));
        }

        public Task CompleteAsync()
        {
            return Task.WhenAll(_completedCallbacks.Select(
                callback => callback.Callback(callback.State)));
        }
    }
}