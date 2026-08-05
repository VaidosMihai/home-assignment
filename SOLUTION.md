# Solution - Home Library Manager

## How to Run

Ensure you have Docker and Docker Compose installed. From the root directory of the project, run:

```bash
docker compose up -d --build

Once the containers are built and running, you can access the application:

Web UI (Angular): http://localhost:4200

API (ASP.NET Core): http://localhost:8080 (Swagger UI available at /swagger)

Architecture & Design Decisions
Vertical Slice: Built as a complete, modern full-stack application featuring a REST API backend, PostgreSQL database, and an Angular standalone frontend UI.

CSV Parsing: Utilized CsvHelper to efficiently stream and parse incoming CSV files line by line. The application gracefully skips the header row and automatically ignores empty or malformed rows without failing the entire batch.

Database & Resilience (Graceful Startup): Used Entity Framework Core with PostgreSQL. Implemented a robust database retry/connection check mechanism at startup so the API safely waits if the PostgreSQL container is still initializing during docker compose up.

Frontend & UX: Developed using Angular (Standalone Components) with plain CSS (no heavy component libraries) for a clean, readable, and responsive layout. Added modern Drag-and-Drop functionality with real-time file validation to ensure only .csv files are accepted.

Containerization: Fully containerized with custom Dockerfile configurations for both the API and frontend, orchestrated cleanly via docker-compose.yml.

Assumptions Made
The CSV file always contains a standard header row (name, author, genre), which is skipped during processing.

The import_date is automatically generated on the server/database side in UTC upon successful insertion.

Small to medium-sized CSV files are expected, allowing direct batch processing within the request lifecycle.

What I Would Improve With More Time
Implement comprehensive automated unit and integration tests for both API endpoints and the CSV parsing service.

Add asynchronous message broker integration (such as RabbitMQ with a background worker service) to handle high-volume bulk imports asynchronously in production scenarios.

Implement client-side pagination and sorting for large library datasets.

```