using FMS.Core.Models;
using FMS.Core.Services;
using FMS.Infrastructure.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Infrastructure.Mocks;

public class MockRoutingServiceFactory : IRoutingServiceFactory
{
    public IRoutingService Create()
    {
        return new MockRoutingService();
    }
}
