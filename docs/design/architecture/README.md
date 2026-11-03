# Implemented architecture

The [class diagram](class-diagram.puml) shows the current source types and
dependency directions across the API, facade, job-management, core, and
infrastructure layers.

The diagrams describe implemented behavior. Job persistence and RabbitMQ
publishing are separate operations; the publisher runs after the repository
write. There is no Outbox implementation.

Production dependency injection uses MongoDB and RabbitMQ implementations.
Development dependency injection uses in-memory/mock implementations.

For operation-by-operation flows, see the [use-case sequence diagrams](../use-cases/README.md).
