using FMS.Core.Models;
using FMS.Core.Services;
using FMS.Facade;
using FMS.JobManagement.Commands;
using FMS.JobManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FMS.Controllers;
[ApiController]
[Route("api/transportjob")]
public class TransportJobController(
    ITransportJobFacade transportJobFacade,
    ICommandExecutor commandExecutor,
    ILogger<TransportJobController> logger) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] TransportJobRequest req,
        CancellationToken cancellationToken = default)
    {
        CreateTransportJobCommand command;
        try
        {
            command = await transportJobFacade.CreateTransportJobAsync(req, cancellationToken);
        }
        catch (CommandValidationException exception)
        {
            return BadRequest(new ValidationProblemDetails(
                exception.Errors.ToDictionary(error => error.Key, error => error.Value)));
        }

        Response.OnCompleted(async () =>
        {
            try
            {
                await commandExecutor.EnqueueAsync(command, CancellationToken.None);
            }
            catch (Exception exception)
            {
                logger.LogCritical(
                    exception,
                    "Accepted create command for job {JobId} could not be dispatched.",
                    command.JobId);
                throw;
            }
        });

        return AcceptedAtAction(
            nameof(GetById),
            new { jobId = command.JobId },
            new
            {
                message = "Job accepted for processing",
                jobId = command.JobId,
                orderId = command.OrderId,
                status = "Accepted"
            });
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Job>>> GetAll()
    {
        var jobs = await transportJobFacade.GetAllJobsAsync();
        return Ok(jobs);
    }
    [HttpGet("{jobId}")]
    public async Task<IActionResult> GetById(string jobId)
    {
        var job = await transportJobFacade.GetJobAsync(jobId);
        if (job == null)
        {
            return NotFound(new { Message = $"Transport job with ID '{jobId}' not found." });
        }

        return Ok(job);
    }
}
