using FMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Core.Services;

public interface IRoutingService
{
    Task<RouteInfo> GetRouteAsync(string origin, string destination, CancellationToken cancellationToken = default);
}
