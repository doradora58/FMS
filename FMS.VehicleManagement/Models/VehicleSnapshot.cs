namespace FMS.VehicleManagement.Models;

public sealed record VehicleSnapshot(
    string Manufacturer,
    string SerialNumber,
    string ConnectionState,
    VehiclePosition? Position,
    DateTimeOffset LastSeenAt,
    DateTimeOffset? PositionUpdatedAt);
