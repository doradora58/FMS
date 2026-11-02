using FMS.Core.Models;
using FMS.Core.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Infrastructure.External;

public class GoogleMapsRoutingService(HttpClient httpClient, IConfiguration configuration) : IRoutingService
{
    public async Task<RouteInfo> GetRouteAsync(string origin, string destination, CancellationToken cancellationToken = default)
    {
        var apiKey = configuration["GoogleMaps:ApiKey"]
                     ?? throw new InvalidOperationException("Google Maps API Key is not configured.");

        var url = "https://routes.googleapis.com/directions/v2:computeRoutes";

        var requestBody = new RoutesApiRequest
        {
            Origin = new Waypoint { Address = origin },
            Destination = new Waypoint { Address = destination }
        };

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        requestMessage.Content = JsonContent.Create(requestBody);

        requestMessage.Headers.Add("X-Goog-Api-Key", apiKey);
        requestMessage.Headers.Add("X-Goog-FieldMask", "routes.distanceMeters,routes.duration");

        var response = await httpClient.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Routes API Error: {response.StatusCode} - {errorContent}");
        }

        var apiResponse = await response.Content.ReadFromJsonAsync<RoutesApiResponse>(cancellationToken: cancellationToken);
        var route = apiResponse?.Routes?.FirstOrDefault();

        if (route == null)
        {
            throw new InvalidOperationException($"Could not calculate route between '{origin}' and '{destination}' using Routes API.");
        }

        int durationSeconds = 0;
        if (!string.IsNullOrEmpty(route.Duration) && route.Duration.EndsWith('s'))
        {
            var numericPart = route.Duration[..^1];
            int.TryParse(numericPart, out durationSeconds);
        }

        return new RouteInfo(
            DistanceMeters: route.DistanceMeters,
            DurationSeconds: durationSeconds
        );
    }
}
