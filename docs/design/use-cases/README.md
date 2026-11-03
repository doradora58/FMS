# Use cases

Each use case has a dedicated sequence diagram based on the implemented source.
Job creation responds with `202 Accepted` and a job ID before the command is
enqueued by the response-completion callback. Background processing creates
the job, so an immediate GET may return `404` until that insertion completes.
The in-memory command queue is not durable; a process failure can lose accepted
work.

| Use case | Entry point | Sequence |
|---|---|---|
| Create a transport job | `POST /api/transportjob/create` | [Create transport job](create-transport-job/sequence.puml) |
| Get a transport job | `GET /api/transportjob/{jobId}` | [Get transport job](get-transport-job/sequence.puml) |
| List transport jobs | `GET /api/transportjob` | [List transport jobs](list-transport-jobs/sequence.puml) |
| Transition job status | `TransportJobFacade.ProgressJobAsync` | [Transition job status](transition-job-status/sequence.puml) |

The status transition is implemented by the facade and job-management service,
but is not currently exposed as a controller HTTP endpoint. Its sequence starts
at the application boundary and does not imply an API route exists.
