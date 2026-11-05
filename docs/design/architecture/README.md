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

Vehicle integration is isolated in the `FMS.VehicleManagement` class library,
which owns vehicle snapshots, the in-memory registry, and VDA 5050 message
parsing. Production starts an MQTT background receiver that subscribes to
`uagv/v2/+/+/state`, `uagv/v2/+/+/visualization`, and
`uagv/v2/+/+/connection`. The vehicle list is exposed at `GET /api/vehicles`
and the dashboard refreshes its Google Maps markers every three seconds.
Development uses a sample vehicle marker and does not require an MQTT broker.
The development MQTT receiver can be enabled explicitly with
`Vda5050__Enabled=true` when a broker is available.
When the browser Maps API key is absent in Development, the dashboard displays
a lightweight mock-map panel with an animated vehicle icon instead of hiding
the vehicle data behind the Google Maps configuration message.

VDA 5050 2.0.0 position coordinates are normally map-local `x`/`y`; they are
not Google Maps coordinates. This initial integration expects the vehicle
manufacturer's GPS extension as numeric `agvPosition.latitude` and
`agvPosition.longitude` fields. State messages without those extension fields
still update the vehicle's last-seen time, but do not replace its last known
GPS position. Configure the RabbitMQ MQTT plugin/listener before production
use; the existing AMQP connection used for job events is separate. Broker
settings are `Vda5050:Broker:Host`, `Port`, `Username`, `Password`, `UseTls`,
and `ClientId`, with `Vda5050:TopicRoot` defaulting to `uagv/v2`. Credentials
should be supplied through deployment secrets/environment variables.

The dashboard uses the Google Maps JavaScript API to draw the route returned by
Google Routes. Route polyline and endpoint coordinates are stored with each
job. Configure a separate, HTTP-referrer-restricted
`GoogleMaps__JavaScriptApiKey`; the server-side `GoogleMaps__ApiKey` must never
be sent to the browser. The development mock supplies a sample Tokyo route.

For operation-by-operation flows, see the [use-case sequence diagrams](../use-cases/README.md).
