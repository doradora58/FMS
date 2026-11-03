# Design documentation

## Structure

```text
docs/
  README.md
  design/
    README.md
    architecture/
      README.md
      class-diagram.puml
    use-cases/
      README.md
      create-transport-job/
        sequence.puml
      get-transport-job/
        sequence.puml
      list-transport-jobs/
        sequence.puml
      transition-job-status/
        sequence.puml
```

The architecture section documents implemented source structure. Each use case
has its own directory with its sequence diagram. Keep diagrams aligned with the
code, and do not place unimplemented proposals alongside implemented design
documents.

The current system diagrams are maintained separately in the repository-root
[`current-design.puml`](../../current-design.puml). Update them when the
implemented architecture changes.

## Design documents

- [Implemented architecture](architecture/README.md)
- [Use cases](use-cases/README.md)
