# Home Library - Take-Home Assignment

Welcome! **Fork this repository** and build your solution in your fork. This starter gives you the
infrastructure (PostgreSQL via Docker Compose) and sample data, so you can spend your time on the app
rather than the plumbing.

**IMPORTANT: After forking the repository, set it to private and grant read access to `marianchelmus`.**

> **Target effort: ~6 hours.** We value a small, clean, working solution over a large unfinished one.
> If something is unclear, make a reasonable assumption, note it, and keep moving.

**When you are done with the assignment, email us with the link to your fork so we can review it.**

---

## What you'll build

A small **bulk-import** web app for a book library:

1. A user uploads a **CSV of books** in the web UI.
2. The **API** parses the rows and stores each book in **PostgreSQL**.
3. The UI shows a **list of imported books** - name, author, genre, and import date.

The whole thing comes up with a single **`docker compose up -d --build`**.

That's the core: a small, complete vertical slice - **upload → parse → store → list**. We care far more
about clean, correct, well-reasoned code than about feature count. **Polish beyond the requirements is
not expected.**

> There's an optional **messaging bonus** (RabbitMQ + a separate worker) at the end - only if you have
> time. The core assignment does **not** need it.

---

## What's in this starter

| File | Purpose |
|---|---|
| `docker-compose.yml` | PostgreSQL, ready to run, with **commented service templates** for your API and web app to fill in (plus a clearly-marked bonus section for RabbitMQ + a worker). |
| `.env.example` | Environment variables used by the compose file (copy to `.env`). |
| `db/init.sql` | An **optional** reference schema for the `library` table - use it, or use EF Core migrations. |
| `samples/sample-books-10.csv`, `samples/sample-books-20.csv` | Sample data to import (`name`, `author`, `genre`). |

## Quick start

```bash
# 1. copy the env defaults
cp .env.example .env

# 2. bring up the infrastructure (Postgres)
docker compose up -d
```

- PostgreSQL → `localhost:5432` &nbsp;(`library` / `library`, database `library`)

Build your API and web app. Once they're containerised, **edit the `api` / `web` templates** in
`docker-compose.yml` so that a single **`docker compose up -d --build`** brings up the whole application.

---

## Requirements

### Server (.NET / ASP.NET Core)

1. Build a REST API with **ASP.NET Core** (C#). **.NET 8 or newer** is fine - we're on .NET 10.
2. Implement these endpoints:
   - `POST /api/imports` - accepts an uploaded CSV (`multipart/form-data`), parses the rows, and
     **stores each book in PostgreSQL**. Return the number of rows imported (e.g. `200 OK` with a small
     JSON body like `{ "imported": 10 }`).
   - `GET /api/books` - returns the books in the `library` table (name, author, genre, import date),
     ordered by import date descending.
3. The CSV has three columns - **name**, **author**, **genre** - with a header row (see the provided
   `samples/sample-books-10.csv` and `samples/sample-books-20.csv`).
4. Handle the obvious errors sensibly - no file, wrong content type, empty CSV - with appropriate
   status codes.
5. Data access is your call - **EF Core** (matches our stack), Dapper, or Npgsql. A small CSV library
   (e.g. CsvHelper) is fine too, or parse the rows yourself.

### Database (PostgreSQL)

1. A single table, **`library`**, with at least: an id, `name`, `author`, `genre`, and `import_date`.
2. `import_date` is the moment the book was imported (set when the row is inserted), stored in UTC.
3. How the schema is created is your call: an EF migration or the provided `db/init.sql` init script
   are both fine.

### Client (web UI)

1. Build a small web UI. **Use whatever framework you're comfortable with** - Angular, React, and Vue
   are all fine. (Angular is our stack, so it's a plus, but it is **not** required.)
2. Implement these features:
   - A **file upload** (click-to-browse is all you need) that uploads a CSV to `POST /api/imports`.
   - A **list/table of books** showing **name, author, genre, and import date**, populated from
     `GET /api/books`.
   - After an upload, refresh the list so the newly imported books show up.
3. Keep it clean and readable. **Styling is up to you** - a little plain CSS is plenty; no component
   library is required.

### Docker

1. Extend the provided `docker-compose.yml` so it runs the **entire application** - PostgreSQL, the API,
   and the web app - so that a single **`docker compose up -d --build`** brings everything up and the
   app is usable in the browser with no further steps.
2. Include a `Dockerfile` for the API and for the frontend.
3. For local development you may also run the services directly (`dotnet run`, and your frontend's dev
   server), but the path we grade is the single-command `docker compose up -d --build`.

---

## Assumptions You Can Make

To keep you moving - make these calls without overthinking them (and feel free to note your own):

- The CSV always has a header row; skip it.
- The `id` type is your choice (e.g. integer identity or UUID).
- Skip malformed or incomplete rows; the valid rows should still import. No de-duplication is required.
- Assume small CSVs (tens to low-hundreds of rows) - no batching, streaming, or performance tuning
  needed.
- No pagination, filtering, auth, or editing/deleting is required - just import and list.

---

## Bonus Points

These are **optional**. A clean, working core beats a half-finished pile of bonuses - don't start these
until the core works end to end.

1. **Messaging with RabbitMQ (the big one).** Instead of the API writing to the database directly, have
   it **publish** each parsed row to a RabbitMQ queue (one small `{ name, author, genre }` message per
   row). Then add a **separate worker service** that **consumes** those messages and inserts each book,
   stamping `import_date`. This producer → queue → consumer split (the worker, not the API, does the DB
   writes) is exactly how we build things in production. With it, `GET /api/books` becomes eventually
   consistent - books appear a moment after upload - so have the UI **poll every couple of seconds**
   after an upload instead of refreshing once. A `rabbitmq` service and the matching `worker` / `api`
   env vars are stubbed in `docker-compose.yml` and `.env.example` to get you started.
2. **Drag-and-drop upload** - a drop area that highlights on drag-over and rejects non-CSV files.
3. **A few unit tests.**
4. **Graceful startup** - retry the PostgreSQL (and RabbitMQ, if you added it) connection on startup
   when the dependency isn't ready yet.

---

## What We're Looking For

- A **working, easy-to-run app** over a feature-packed broken one.
- **Clear, readable code** with sensible types (records/DTOs) and a tidy project layout.
- A clean end-to-end slice - **upload → parse → store → list** - wired together so a single
  `docker compose up -d --build` brings it all up.

---

## Definition of Done

A quick self-check before you submit:

1. Uploading a CSV in the UI results in the books appearing in the list, each with name, author, genre,
   and import date.
2. A single `docker compose up -d --build` brings up the entire app (PostgreSQL, API, and UI), usable in
   the browser with no further steps.
3. Obvious errors (no file, empty CSV) are handled with sensible status codes.

---

## Submission

1. **Fork this repository**, change the repository visibility to PRIVATE and build your solution in your fork.
2. Add a short **`SOLUTION.md`** (or a section at the top of this README) covering: how to run it, any
   assumptions or design decisions you made, and anything you'd improve with more time.
3. Make sure `docker compose up -d --build` brings up the whole stack.
4. Send us the link to your fork. If your fork is private, grant read access to **`marianchelmus`**.

---

## Evaluation Criteria

1. Code quality and clarity.
2. Functionality and completeness for the given scope.
3. A clean end-to-end vertical slice (upload → parse → store → list).
4. Usability and overall polish.
5. Proper use of C# / .NET, your chosen frontend, PostgreSQL, and 3rd-party packages.

---

Good luck - we're looking forward to seeing how you think.
