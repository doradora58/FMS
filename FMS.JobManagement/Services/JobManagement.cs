using FMS.Core.Models;
using FMS.Core.Repositories;
using FMS.JobManagement.Models;
using FMS.JobManagement.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.JobManagement.Services;

public class JobManagement(ITransportJobRepository jobRepository,
    IJobFactory jobFactory):IJobManagement
{
    public async Task<IJob> CreateAndSaveJobAsync(string sourceOrderId, string fromAddress, string toAddress, double distance, int duration, CancellationToken cancellationToken = default)
    {
        var job = jobFactory.Create(sourceOrderId, fromAddress, toAddress, distance, duration);
        await jobRepository.AddAsync(job);
        return job;
    }

    public async Task TransitionStatusAsync(string jobId, JobTrigger trigger, CancellationToken cancellationToken = default)
    {
        var job = await jobRepository.GetAsync(jobId) ?? throw new KeyNotFoundException($"Job {jobId} not found.");

        if (!Enum.TryParse<JobStatus>(job.Status, out var currentStatus))
        {
            currentStatus = JobStatus.Queued;
        }

        var machine = new JobStateMachine(currentStatus);
        machine.Fire(trigger);

        job.Status = machine.CurrentState.ToString();
        await jobRepository.UpdateAsync(job);
    }

    public async Task<IEnumerable<IJob>> GetAllJobsAsync(CancellationToken cancellationToken = default)
    {
        return await jobRepository.GetAllAsync();
    }

    public async Task<IJob?> GetJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        return await jobRepository.GetAsync(jobId);
    }
}
