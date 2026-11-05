using System.Diagnostics;

namespace FMS.JobManagement.Telemetry;

public static class JobActivity
{
    public const string SourceName = "Fms.JobManagement";

    public static readonly ActivitySource Source = new(SourceName);
}
