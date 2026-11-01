using FMS.Core.Models;
using FMS.Core.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Infrastructure.Mocks;

public class MockMessagePublisher(ILogger<MockMessagePublisher> logger) : IMessagePublisher
{
    public async Task PublishJobStatusAsync(JobStatusEvent jobEvent)
    {
        logger.LogInformation("[MOCK] Message published (Simulation): {Message}", jobEvent);
    }
}
