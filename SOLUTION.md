# Solution - Home Library Manager

## How to Run

Ensure Docker and Docker Compose are installed. From the project root, run:

```bash
docker compose up -d --build
```

Once the containers are running, the application is available at:

- Web UI: http://localhost:4200
- API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger
- RabbitMQ Management UI: http://localhost:15672

## Architecture & Design Decisions

This solution follows a vertical-slice architecture built around a full-stack workflow:

- Angular standalone frontend for upload and table visualization
- ASP.NET Core minimal API for CSV import and book listing
- PostgreSQL database through EF Core
- RabbitMQ producer/worker bonus flow for asynchronous import persistence

### CSV Parsing
The upload flow uses `CsvHelper` to stream CSV rows safely and skip the metadata header row. Empty or malformed rows are ignored without breaking the batch import.

### Messaging Bonus
The API now publishes parsed rows into a RabbitMQ queue, and a dedicated worker consumes those messages and persists them into PostgreSQL. This enables asynchronous processing and a clean producer/consumer separation.

### Frontend Behavior
The Angular UI supports drag-and-drop upload validation for `.csv` files. After a successful import, the frontend polls the book list every 2 seconds to reflect the eventual database state.

### Database & Startup Resilience
The application uses EF Core with PostgreSQL and performs startup schema creation safely so the API can tolerate the database container still initializing during `docker compose up`.

### Containerization
The stack is orchestrated through `docker-compose.yml` with separate services for:

- API
- Web UI
- PostgreSQL
- RabbitMQ
- Background worker

## Assumptions

- The CSV input always contains a standard header row in the order `name, author, genre`.
- The header row is intentionally skipped during parsing.
- `import_date` is generated on the server side in UTC once a row is inserted.
- The application is intended for small to medium CSV imports in the core assignment, while RabbitMQ is used as a production-style asynchronous bonus.

## Tests Added

A dedicated `HomeLibrary.Tests` project was added with unit tests covering CSV parsing behavior for:

- valid rows parsing
- skipping of empty or malformed rows
- object initialization expectations

## Possible Future Improvements

- Add integration tests for the API import endpoint and worker persistence flow
- Expand observability with structured logs and queue metrics
- Add pagination and sorting for larger book datasets

```