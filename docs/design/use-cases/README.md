# Use cases

Each use case has a dedicated sequence diagram based on the implemented source.

| Use case | Entry point | Sequence |
|---|---|---|
| Create a transport job | `POST /api/transportjob/create` | [Create transport job](create-transport-job/sequence.puml) |
| Get a transport job | `GET /api/transportjob/{jobId}` | [Get transport job](get-transport-job/sequence.puml) |
| List transport jobs | `GET /api/transportjob` | [List transport jobs](list-transport-jobs/sequence.puml) |
| Transition job status | `TransportJobFacade.ProgressJobAsync` | [Transition job status](transition-job-status/sequence.puml) |

The status transition is implemented by the facade and job-management service,
but is not currently exposed as a controller HTTP endpoint. Its sequence starts
at the application boundary and does not imply an API route exists.
