namespace FMS.Models
{
    public class DispatchRequest
    {
        public string OrderId { get; set; } = null!;
        public string FromAddress { get; set; } = null!;
        public string ToAddress { get; set; } = null!;
    }
}
