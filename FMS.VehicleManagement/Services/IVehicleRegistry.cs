using FMS.VehicleManagement.Models;

namespace FMS.VehicleManagement.Services;

public interface IVehicleRegistry
{
    IReadOnlyCollection<VehicleSnapshot> GetVehicles();

    VehicleSnapshot UpdateState(VehicleStateUpdate update);

    VehicleSnapshot UpdateConnection(
        string manufacturer,
        string serialNumber,
        string connectionState,
        DateTimeOffset receivedAt);
}
