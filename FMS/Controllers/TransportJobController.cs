using FMS.Core.Models;
using FMS.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FMS.Controllers;
[ApiController]
[Route("api/transportjob")]
public class TransportJobController(ITransportJobService transportJobService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] TransportJobRequest req)
    {
        var job = await transportJobService.CreateTransportJobAsync(req);
        return Ok(new { message = "Job created and queued", jobId = job.JobId, orderId = req.OrderId });
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Job>>> GetAll()
    {
        var jobs = await transportJobService.GetAllJobsAsync();
        return Ok(jobs);
    }
    [HttpGet("{jobId}")]
    public async Task<IActionResult> GetById(string jobId)
    {
        var job = await transportJobService.GetJobAsync(jobId);
        if (job == null)
        {
            return NotFound(new { Message = $"Transport job with ID '{jobId}' not found." });
        }

        return Ok(job);
    }
}
