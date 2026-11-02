using FMS.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Infrastructure.Factories;

public interface IRoutingServiceFactory
{
    public IRoutingService Create();
}
