using FMS.Models;
using FMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace FMS.Controllers;
[ApiController]
[Route("api")]
public class JobController(IJobService jobService) : ControllerBase
{
    [HttpPost("dispatch")]
    public async Task<IActionResult> Dispatch([FromBody] DispatchRequest req)
    {
        var job = await jobService.CreateJobFromDispatchAsync(req);
        return Ok(new { message = "Job created and queued", jobId = job.JobId, orderId = req.OrderId });
    }

    [HttpGet("jobs")]
    public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
    {
        var jobs = await jobService.GetJobsAsync();
        return Ok(jobs);
    }
}
