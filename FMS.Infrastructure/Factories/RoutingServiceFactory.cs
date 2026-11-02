using FMS.Core.Services;
using FMS.Infrastructure.External;
using FMS.Infrastructure.Mocks;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Infrastructure.Factories;

public class RoutingServiceFactory(IConfiguration configuration, HttpClient httpClient) : IRoutingServiceFactory
{
    public IRoutingService Create()
    {
        return new GoogleMapsRoutingService(httpClient, configuration);
    }

}