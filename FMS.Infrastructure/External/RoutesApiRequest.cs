using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FMS.Infrastructure.External;

public class RoutesApiRequest
{
    [JsonPropertyName("origin")]
    public Waypoint Origin { get; set; } = new();

    [JsonPropertyName("destination")]
    public Waypoint Destination { get; set; } = new();

    [JsonPropertyName("travelMode")]
    public string TravelMode { get; set; } = "DRIVE";
}

public class Waypoint
{
    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;
}

public class RoutesApiResponse
{
    [JsonPropertyName("routes")]
    public List<RouteItem> Routes { get; set; } = [];
}
public class RouteItem
{
    [JsonPropertyName("distanceMeters")]
    public int DistanceMeters { get; set; }

    [JsonPropertyName("duration")]
    public string Duration { get; set; } = string.Empty;
}