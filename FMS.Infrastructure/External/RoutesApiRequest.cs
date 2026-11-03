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

    [JsonPropertyName("polyline")]
    public RoutePolyline? Polyline { get; set; }

    [JsonPropertyName("legs")]
    public List<RouteLeg> Legs { get; set; } = [];
}

public class RoutePolyline
{
    [JsonPropertyName("encodedPolyline")]
    public string EncodedPolyline { get; set; } = string.Empty;
}

public class RouteLeg
{
    [JsonPropertyName("startLocation")]
    public RouteLocation? StartLocation { get; set; }

    [JsonPropertyName("endLocation")]
    public RouteLocation? EndLocation { get; set; }
}

public class RouteLocation
{
    [JsonPropertyName("latLng")]
    public RouteLatLng? LatLng { get; set; }
}

public class RouteLatLng
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}