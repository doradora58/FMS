using System.Collections.Concurrent;
using FMS.VehicleManagement.Models;

namespace FMS.VehicleManagement.Services;

public sealed class InMemoryVehicleRegistry : IVehicleRegistry
{
    private readonly ConcurrentDictionary<string, VehicleSnapshot> vehicles = new(StringComparer.Ordinal);

    public IReadOnlyCollection<VehicleSnapshot> GetVehicles() =>
        vehicles.Values
            .OrderBy(vehicle => vehicle.Manufacturer, StringComparer.Ordinal)
            .ThenBy(vehicle => vehicle.SerialNumber, StringComparer.Ordinal)
            .ToArray();

    public VehicleSnapshot UpdateState(VehicleStateUpdate update) =>
        vehicles.AddOrUpdate(
            GetKey(update.Manufacturer, update.SerialNumber),
            _ => new VehicleSnapshot(
                update.Manufacturer,
                update.SerialNumber,
                "Unknown",
                update.Position,
                update.ReceivedAt,
                update.Position is null ? null : update.ReceivedAt),
            (_, current) => current with
            {
                Position = update.Position ?? current.Position,
                LastSeenAt = update.ReceivedAt,
                PositionUpdatedAt = update.Position is null
                    ? current.PositionUpdatedAt
                    : update.ReceivedAt
            });

    public VehicleSnapshot UpdateConnection(
        string manufacturer,
        string serialNumber,
        string connectionState,
        DateTimeOffset receivedAt) =>
        vehicles.AddOrUpdate(
            GetKey(manufacturer, serialNumber),
            _ => new VehicleSnapshot(
                manufacturer,
                serialNumber,
                connectionState,
                null,
                receivedAt,
                null),
            (_, current) => current with
            {
                ConnectionState = connectionState,
                LastSeenAt = receivedAt
            });

    private static string GetKey(string manufacturer, string serialNumber) =>
        $"{manufacturer}\0{serialNumber}";
}
