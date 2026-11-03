using FMS.Infrastructure.Database;
using FMS.JobManagement.Models;

namespace FMS.Tests.Infrastructure;

public class JobDocumentTests
{
    [Fact]
    public void GivenJobWithRouteGeometryWhenConvertedToAndFromDocumentThenPreservesRouteData()
    {
        // Arrange
        var job = new Job
        {
            JobId = "JOB-123",
            SourceOrderId = "ORDER-456",
            FromAddress = "Origin",
            ToAddress = "Destination",
            RoutePolyline = "encoded-route",
            OriginLatitude = 35.1,
            OriginLongitude = 139.2,
            DestinationLatitude = 35.4,
            DestinationLongitude = 139.5
        };

        // Act
        var restored = JobDocument.FromDomain(job).ToDomain();

        // Assert
        Assert.Equal(job.RoutePolyline, restored.RoutePolyline);
        Assert.Equal(job.OriginLatitude, restored.OriginLatitude);
        Assert.Equal(job.OriginLongitude, restored.OriginLongitude);
        Assert.Equal(job.DestinationLatitude, restored.DestinationLatitude);
        Assert.Equal(job.DestinationLongitude, restored.DestinationLongitude);
    }
}
