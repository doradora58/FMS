using System.Net;
using System.Text;
using FMS.Infrastructure.External;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace FMS.Tests.Infrastructure;

public class GoogleMapsRoutingServiceTests
{
    [Fact]
    public async Task GivenRouteResponseWithPolylineWhenCalculatedThenReturnsRouteGeometryAndEndpoints()
    {
        // Arrange
        const string responseJson = """
            {
              "routes": [{
                "distanceMeters": 1250,
                "duration": "135s",
                "polyline": { "encodedPolyline": "encoded-route" },
                "legs": [{
                  "startLocation": { "latLng": { "latitude": 35.1, "longitude": 139.2 } },
                  "endLocation": { "latLng": { "latitude": 35.4, "longitude": 139.5 } }
                }]
              }]
            }
            """;
        var handler = new StubHttpMessageHandler(responseJson);
        using var httpClient = new HttpClient(handler);
        var configuration = Substitute.For<IConfiguration>();
        configuration["GoogleMaps:ApiKey"].Returns("server-api-key");
        var service = new GoogleMapsRoutingService(httpClient, configuration);

        // Act
        var route = await service.GetRouteAsync("Origin", "Destination");

        // Assert
        Assert.Equal(1250, route.DistanceMeters);
        Assert.Equal(135, route.DurationSeconds);
        Assert.Equal("encoded-route", route.EncodedPolyline);
        Assert.Equal(35.1, route.OriginLatitude);
        Assert.Equal(139.2, route.OriginLongitude);
        Assert.Equal(35.4, route.DestinationLatitude);
        Assert.Equal(139.5, route.DestinationLongitude);
        Assert.Contains("routes.polyline.encodedPolyline", handler.FieldMask);
        Assert.Contains("routes.legs.startLocation", handler.FieldMask);
        Assert.Contains("routes.legs.endLocation", handler.FieldMask);
    }

    private sealed class StubHttpMessageHandler(string responseJson) : HttpMessageHandler
    {
        public string FieldMask { get; private set; } = string.Empty;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            FieldMask = string.Join(
                ',',
                request.Headers.GetValues("X-Goog-FieldMask"));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });
        }
    }
}
