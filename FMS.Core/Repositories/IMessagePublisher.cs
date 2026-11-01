using FMS.Core.Models;

namespace FMS.Core.Repositories;
public interface IMessagePublisher
{
    Task PublishJobStatusAsync(JobStatusEvent jobEvent);
}