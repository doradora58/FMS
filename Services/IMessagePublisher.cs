using FMS.Models;

namespace FMS.Services
{
    public interface IMessagePublisher
    {
        Task PublishJobStatusAsync(JobStatusEvent jobEvent);
    }
}
