using FMS.Core.Models;
using FMS.Facade;
using FMS.JobManagement.Commands;
using FMS.JobManagement.Services;
using NSubstitute;

namespace FMS.Tests.Facade;

public class TransportJobFacadeTests
{
    [Fact]
    public async Task GivenValidRequestWhenCreateJobThenValidatesAndReturnsCommand()
    {
        // Arrange
        var operations = new List<string>();
        var executor = Substitute.For<ICommandExecutor>();
        var validator = Substitute.For<ICommandValidator<CreateTransportJobCommand>>();
        var queries = Substitute.For<IJobQueries>();
        validator.Validate(Arg.Any<CreateTransportJobCommand>()).Returns(_ =>
        {
            operations.Add("validate");
            return new Dictionary<string, string[]>();
        });
        var facade = new TransportJobFacade(executor, validator, queries);
        var request = new TransportJobRequest
        {
            OrderId = "ORDER-456",
            FromAddress = "A",
            ToAddress = "B"
        };

        // Act
        var command = await facade.CreateTransportJobAsync(request);

        // Assert
        Assert.StartsWith("JOB-", command.JobId);
        Assert.Equal(["validate"], operations);
        Assert.Equal(request.OrderId, command.OrderId);
        Assert.Equal(request.FromAddress, command.FromAddress);
        Assert.Equal(request.ToAddress, command.ToAddress);
        await executor.DidNotReceive().EnqueueAsync(
            Arg.Any<CreateTransportJobCommand>(),
            Arg.Any<CancellationToken>());
        await executor.DidNotReceive().ExecuteAsync<CreateTransportJobCommand, Unit>(
            Arg.Any<CreateTransportJobCommand>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenInvalidRequestWhenCreateJobThenDoesNotEnqueueCommand()
    {
        // Arrange
        var executor = Substitute.For<ICommandExecutor>();
        var validator = Substitute.For<ICommandValidator<CreateTransportJobCommand>>();
        validator.Validate(Arg.Any<CreateTransportJobCommand>()).Returns(
            new Dictionary<string, string[]> { ["FromAddress"] = ["FromAddress is required."] });
        var facade = new TransportJobFacade(executor, validator, Substitute.For<IJobQueries>());
        var request = new TransportJobRequest
        {
            OrderId = "ORDER-456",
            ToAddress = "B"
        };

        // Act
        var exception = await Assert.ThrowsAsync<CommandValidationException>(
            () => facade.CreateTransportJobAsync(request));

        // Assert
        Assert.Contains("FromAddress", exception.Errors.Keys);
    }
}
