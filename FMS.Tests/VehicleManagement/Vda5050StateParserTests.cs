using System.Text;
using FMS.VehicleManagement.Models;
using FMS.VehicleManagement.Services;
using FMS.VehicleManagement.Vda5050;

namespace FMS.Tests.VehicleManagement;

public class Vda5050StateParserTests
{
    [Fact]
    public void GivenVdaStateWithGpsExtensionWhenParsedThenReturnsVehicleAndCoordinates()
    {
        var receivedAt = DateTimeOffset.UtcNow;
        var payload = Encoding.UTF8.GetBytes(
            """{"manufacturer":"Example","serialNumber":"AGV-01","agvPosition":{"latitude":35.681236,"longitude":139.767125}}""");

        var update = Vda5050StateParser.ParseState(
            "uagv/v2/Example/AGV-01/state",
            payload,
            receivedAt);

        Assert.Equal("Example", update.Manufacturer);
        Assert.Equal("AGV-01", update.SerialNumber);
        Assert.Equal(new VehiclePosition(35.681236, 139.767125), update.Position);
        Assert.Equal(receivedAt, update.ReceivedAt);
    }

    [Fact]
    public void GivenStateWithoutGpsExtensionWhenParsedThenPositionIsMissing()
    {
        var payload = Encoding.UTF8.GetBytes(
            """{"manufacturer":"Example","serialNumber":"AGV-01","agvPosition":{"x":1.2,"y":3.4,"theta":0}}""");

        var update = Vda5050StateParser.ParseState(
            "uagv/v2/Example/AGV-01/state",
            payload,
            DateTimeOffset.UtcNow);

        Assert.Null(update.Position);
    }

    [Theory]
    [InlineData("""{"agvPosition":{"latitude":91,"longitude":0}}""")]
    [InlineData("""{"agvPosition":{"latitude":35,"longitude":181}}""")]
    [InlineData("""{"agvPosition":{"latitude":35}}""")]
    public void GivenInvalidGpsCoordinatesWhenParsedThenThrowsFormatException(string json)
    {
        var payload = Encoding.UTF8.GetBytes(json);

        Assert.Throws<FormatException>(() => Vda5050StateParser.ParseState(
            "uagv/v2/Example/AGV-01/state",
            payload,
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void GivenPayloadIdentityDoesNotMatchTopicWhenParsedThenThrowsFormatException()
    {
        var payload = Encoding.UTF8.GetBytes(
            """{"manufacturer":"Other","serialNumber":"AGV-01","agvPosition":{"latitude":35,"longitude":139}}""");

        Assert.Throws<FormatException>(() => Vda5050StateParser.ParseState(
            "uagv/v2/Example/AGV-01/state",
            payload,
            DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData("ONLINE", "Online")]
    [InlineData("OFFLINE", "Offline")]
    [InlineData("CONNECTIONBROKEN", "Offline")]
    public void GivenConnectionMessageWhenParsedThenReturnsConnectionState(string payload, string expectedState)
    {
        var result = Vda5050StateParser.ParseConnection(
            "uagv/v2/Example/AGV-01/connection",
            Encoding.UTF8.GetBytes(payload));

        Assert.Equal(("Example", "AGV-01", expectedState), result);
    }

    [Fact]
    public void GivenStateWithoutGpsAfterPositionWhenStoredThenKeepsLastKnownPosition()
    {
        var registry = new InMemoryVehicleRegistry();
        var position = new VehiclePosition(35.681236, 139.767125);
        var firstSeen = DateTimeOffset.UtcNow;
        registry.UpdateState(new VehicleStateUpdate("Example", "AGV-01", position, firstSeen));

        var laterSeen = firstSeen.AddSeconds(5);
        var current = registry.UpdateState(new VehicleStateUpdate("Example", "AGV-01", null, laterSeen));

        Assert.Equal(position, current.Position);
        Assert.Equal(firstSeen, current.PositionUpdatedAt);
        Assert.Equal(laterSeen, current.LastSeenAt);
    }
}
