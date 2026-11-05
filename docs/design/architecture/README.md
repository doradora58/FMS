# Implemented architecture

The [class diagram](class-diagram.puml) shows the current source types and
dependency directions across the API, facade, job-management, core, and
infrastructure layers.

The facade creates and validates a transport-job command. The controller
returns `202 Accepted` and only enqueues that command from the HTTP response
completion callback. The background command handler creates the `Routing` job,
calculates the route, updates it to `Queued` or `Failed`, and publishes the
resulting status event. No MongoDB operation runs before the HTTP response has
been sent. Read operations use `IJobQueries`.

Command execution uses an in-memory background queue in both production and
development; RabbitMQ is used only to publish external job-status events. The
in-memory queue is not durable. A process failure after returning `202` can
lose a command, including the small window before the response-completion
callback enqueues it. Unexpected handler failures are logged, the command is
dropped, and a job already inserted as `Routing` can remain in that state; no
automatic retry is configured. A route-calculation error is persisted as
`Failed`.

The create command captures the incoming W3C `traceparent` and `tracestate`
before the HTTP response completes. The background worker restores that parent
context, so job creation, database operations, route calculation, and the
status-event publish are spans in the originating request trace. Job-related
spans include `job.id`; RabbitMQ status events carry W3C trace headers for
downstream consumers.

Since background processing creates the job, `GET /api/transportjob/{jobId}`
can temporarily return `404` after the `202 Accepted` response until insertion
completes. Job persistence and status-event publishing are not atomic; there
is no transactional Outbox.

Production dependency injection uses MongoDB, Google Routes, and RabbitMQ for
status-event publishing. Development dependency injection uses in-memory/mock
repository, routing, and publisher implementations.

The dashboard uses the Google Maps JavaScript API to draw the route returned by
Google Routes. Route polyline and endpoint coordinates are stored with each
job. Configure a separate, HTTP-referrer-restricted
`GoogleMaps__JavaScriptApiKey`; the server-side `GoogleMaps__ApiKey` must never
be sent to the browser. The development mock supplies a sample Tokyo route.

For operation-by-operation flows, see the [use-case sequence diagrams](../use-cases/README.md).
