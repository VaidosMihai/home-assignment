-- Optional reference schema for the `library` table.
--
-- Use this ONLY if you prefer a SQL-created schema over EF Core migrations.
-- To enable it, uncomment the init.sql volume mount under the `postgres`
-- service in docker-compose.yml. Postgres runs files in
-- /docker-entrypoint-initdb.d once, on first startup of an empty data volume.

CREATE TABLE IF NOT EXISTS library (
    id          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name        TEXT        NOT NULL,
    author      TEXT        NOT NULL,
    genre       TEXT        NOT NULL,
    import_date TIMESTAMPTZ NOT NULL DEFAULT now()
);
