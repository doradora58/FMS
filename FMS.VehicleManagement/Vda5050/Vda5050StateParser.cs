using System.Text;
using System.Text.Json;
using FMS.VehicleManagement.Models;

namespace FMS.VehicleManagement.Vda5050;

public static class Vda5050StateParser
{
    public static VehicleStateUpdate ParseState(
        string topic,
        ReadOnlyMemory<byte> payload,
        DateTimeOffset receivedAt,
        string topicRoot = "uagv/v2")
    {
        var (manufacturer, serialNumber) = ParseVehicleIdentity(topic, topicRoot, "state");
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("VDA 5050 state payload must be a JSON object.");
        }

        ValidateIdentity(root, "manufacturer", manufacturer);
        ValidateIdentity(root, "serialNumber", serialNumber);

        VehiclePosition? position = null;
        if (root.TryGetProperty("agvPosition", out var agvPosition) &&
            agvPosition.ValueKind == JsonValueKind.Object)
        {
            var hasLatitude = agvPosition.TryGetProperty("latitude", out var latitude);
            var hasLongitude = agvPosition.TryGetProperty("longitude", out var longitude);
            if (hasLatitude != hasLongitude)
            {
                throw new FormatException("VDA 5050 agvPosition must provide both latitude and longitude.");
            }

            if (hasLatitude)
            {
                var latitudeValue = ReadCoordinate(latitude, "latitude", -90, 90);
                var longitudeValue = ReadCoordinate(longitude, "longitude", -180, 180);
                position = new VehiclePosition(latitudeValue, longitudeValue);
            }
        }

        return new VehicleStateUpdate(manufacturer, serialNumber, position, receivedAt);
    }

    public static (string Manufacturer, string SerialNumber, string ConnectionState) ParseConnection(
        string topic,
        ReadOnlyMemory<byte> payload,
        string topicRoot = "uagv/v2")
    {
        var (manufacturer, serialNumber) = ParseVehicleIdentity(topic, topicRoot, "connection");
        var state = Encoding.UTF8.GetString(payload.Span).Trim().Trim('"');
        var connectionState = state switch
        {
            "ONLINE" => "Online",
            "OFFLINE" or "CONNECTIONBROKEN" => "Offline",
            _ => throw new FormatException($"Unknown VDA 5050 connection state '{state}'.")
        };

        return (manufacturer, serialNumber, connectionState);
    }

    private static (string Manufacturer, string SerialNumber) ParseVehicleIdentity(
        string topic,
        string topicRoot,
        string messageType)
    {
        var rootSegments = topicRoot.Trim('/').Split('/');
        var topicSegments = topic.Split('/');
        if (topicSegments.Length != rootSegments.Length + 3 ||
            !topicSegments.Take(rootSegments.Length).SequenceEqual(rootSegments, StringComparer.Ordinal) ||
            !string.Equals(topicSegments[^1], messageType, StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(topicSegments[^3]) ||
            string.IsNullOrWhiteSpace(topicSegments[^2]))
        {
            throw new FormatException($"Invalid VDA 5050 {messageType} topic '{topic}'.");
        }

        return (topicSegments[^3], topicSegments[^2]);
    }

    private static void ValidateIdentity(JsonElement root, string propertyName, string topicValue)
    {
        if (!root.TryGetProperty(propertyName, out var value))
        {
            return;
        }

        if (value.ValueKind != JsonValueKind.String ||
            !string.Equals(value.GetString(), topicValue, StringComparison.Ordinal))
        {
            throw new FormatException(
                $"VDA 5050 '{propertyName}' does not match the vehicle identity in the topic.");
        }
    }

    private static double ReadCoordinate(JsonElement value, string name, double minimum, double maximum)
    {
        if (value.ValueKind != JsonValueKind.Number ||
            !value.TryGetDouble(out var coordinate) ||
            !double.IsFinite(coordinate) ||
            coordinate < minimum ||
            coordinate > maximum)
        {
            throw new FormatException($"VDA 5050 GPS {name} is missing or out of range.");
        }

        return coordinate;
    }
}
