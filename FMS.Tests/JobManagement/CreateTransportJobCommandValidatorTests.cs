using FMS.JobManagement.Commands;

namespace FMS.Tests.JobManagement;

public class CreateTransportJobCommandValidatorTests
{
    [Fact]
    public void GivenMissingRequiredFieldsWhenValidatedThenReturnsFieldErrors()
    {
        // Arrange
        var validator = new CreateTransportJobCommandValidator();
        var command = new CreateTransportJobCommand("", "", " ", "");

        // Act
        var errors = validator.Validate(command);

        // Assert
        Assert.Equal(4, errors.Count);
        Assert.Contains(nameof(command.JobId), errors.Keys);
        Assert.Contains(nameof(command.OrderId), errors.Keys);
        Assert.Contains(nameof(command.FromAddress), errors.Keys);
        Assert.Contains(nameof(command.ToAddress), errors.Keys);
    }

    [Fact]
    public void GivenCompleteCommandWhenValidatedThenReturnsNoErrors()
    {
        // Arrange
        var validator = new CreateTransportJobCommandValidator();
        var command = new CreateTransportJobCommand("JOB-123", "ORDER-456", "A", "B");

        // Act
        var errors = validator.Validate(command);

        // Assert
        Assert.Empty(errors);
    }
}
