namespace FMS.VehicleManagement.Models;

public sealed record VehicleStateUpdate(
    string Manufacturer,
    string SerialNumber,
    VehiclePosition? Position,
    DateTimeOffset ReceivedAt);
