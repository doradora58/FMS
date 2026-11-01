namespace FMS.Models;
public record JobStatusEvent(
    string JobId,
    string OrderId,
    string Status,
    DateTime Timestamp
);