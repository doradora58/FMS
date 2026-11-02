using FMS.Core.Models;
using FMS.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Infrastructure.Mocks;

public class MockRoutingService(RouteInfo? routeInfo =null) : IRoutingService
{
    public Task<RouteInfo> GetRouteAsync(string origin, string destination, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(routeInfo?? new  RouteInfo(DistanceMeters: 10000, DurationSeconds: 600));
    }
}
