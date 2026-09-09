# Book Catalog API

A REST API for a small library: a catalogue of books with authors, and lending — a user
borrows a book, returns it, and the history stays visible.

Built with .NET 10, ASP.NET Core, EF Core and PostgreSQL. Runs in Docker.

---

## Quick start

You need [Docker Desktop](https://www.docker.com/products/docker-desktop/). Nothing else.

```bash
git clone <repository-url>
cd BookingCatalog
```

Create a `.env` file next to `docker-compose.yml` with the database password:

```bash
echo "POSTGRES_PASSWORD=devpassword" > .env
```

Then bring the whole thing up:

```bash
docker compose up --build
```

That starts PostgreSQL, waits until it is genuinely accepting connections, then starts the
API, which applies its migrations and seeds a few books. When it settles:

| What | Where |
|---|---|
| Swagger UI | http://localhost:8080/swagger |
| Liveness | http://localhost:8080/health/live |
| Readiness | http://localhost:8080/health/ready |
| API root | http://localhost:8080/api/books |

Stop it with `Ctrl+C`, or `docker compose down`. Add `-v` to also throw away the database
volume and start from a clean schema next time:

```bash
docker compose down -v
```

---

## Running without Docker

You still need a PostgreSQL instance. Point the app at it and run:

```bash
docker compose up -d db
dotnet run --project BookCatalog.Api
```

The connection string in `appsettings.json` already targets `localhost:5432` with the
development password. See [Configuration](#configuration) for overriding it.

---

## API

All responses are JSON. Errors use [ProblemDetails](https://datatracker.ietf.org/doc/html/rfc7807)
so the shape is the same everywhere.

### Books

| Method | Route | Purpose | Success | Failure |
|---|---|---|---|---|
| `GET` | `/api/books` | List, paged and filtered | `200` | `400` bad query |
| `GET` | `/api/books/{id}` | One book | `200` | `404` |
| `POST` | `/api/books` | Create | `201` + `Location` | `400` |
| `PUT` | `/api/books/{id}` | Replace | `200` | `400`, `404` |
| `DELETE` | `/api/books/{id}` | Remove | `204` | `404` |

`GET /api/books` accepts `authorId`, `genre`, `publicationYear`, `page` (default `1`) and
`pageSize` (default `10`, max `100`). Filters combine, and they are applied in SQL — not
after loading the table into memory.

```bash
curl "http://localhost:8080/api/books?genre=Science&page=1&pageSize=2"
```

```json
{
  "books": [
    { "id": "b1111111-...", "title": "Dune", "authorId": "a1111111-...", "genre": 1, "publicationYear": 1965 }
  ],
  "totalBooks": 3
}
```

`totalBooks` is the count of everything matching the filter, not the size of this page — the
client needs it to know how many pages exist.

### Lending

| Method | Route | Purpose | Success | Failure |
|---|---|---|---|---|
| `POST` | `/api/loans` | Borrow a book | `200` + the loan | `404` no such book, `409` already borrowed |
| `POST` | `/api/loans/{loanId}/return` | Return it | `200` | `404` no such loan, `409` already returned |
| `GET` | `/api/users/{userId}/loans` | Borrowing history | `200` | — |

```bash
curl -X POST http://localhost:8080/api/loans \
  -H "Content-Type: application/json" \
  -d '{"userId":"c1111111-1111-1111-1111-111111111111","bookId":"b1111111-1111-1111-1111-111111111111"}'
```

The same book cannot be on loan to two people at once. That rule is enforced by a unique
index in the database rather than by a check in C#, so it holds even when two requests
arrive at the same moment. The loser of the race gets `409`.

### Seeded data

There are no endpoints for creating authors or users yet, so use the ids that ship with the
seed migration:

| Authors | | Users | |
|---|---|---|---|
| Frank Herbert | `a1111111-1111-1111-1111-111111111111` | Alice Doe | `c1111111-1111-1111-1111-111111111111` |
| Stephen King | `a2222222-2222-2222-2222-222222222222` | Bob Smith | `c2222222-2222-2222-2222-222222222222` |
| Isaac Asimov | `a3333333-3333-3333-3333-333333333333` | | |

Five books are seeded too. One of them (`Foundation`) is already on loan to Bob, so trying
to borrow it is an easy way to see the `409`.

---

## Configuration

Configuration is read from `appsettings.json`, then `appsettings.{Environment}.json`, then
environment variables — each layer overriding the one before it.

| Setting | Environment variable | Notes |
|---|---|---|
| Connection string | `ConnectionStrings__BookCatalog` | Required. The app refuses to start without it. |
| Environment | `ASPNETCORE_ENVIRONMENT` | `Development` enables Swagger and startup migrations |
| Port | `ASPNETCORE_HTTP_PORTS` | `8080` in Compose |

The double underscore is how you express a nested key as an environment variable —
`ConnectionStrings__BookCatalog` is the same key as `ConnectionStrings:BookCatalog`.

If the connection string is missing the process throws at startup with a message naming the
key. It does not start and then fail on the first request.

`.env` is gitignored and holds the local database password. The password in
`appsettings.json` is a local development value only; anything real belongs in an
environment variable or a secret store.

---

## Tests

Docker must be running — the integration tests start their own throwaway PostgreSQL
container.

```bash
dotnet test BookCatalog.slnx
```

Pass the solution file explicitly. A bare `dotnet test` fails, because `docker-compose.dcproj`
sits in the same folder and MSBuild cannot tell which one you meant.

| Project | Count | What it covers |
|---|---|---|
| `BookCatalog.Tests` | 37 | Domain rules, service logic and validation, with the repository substituted. No I/O. |
| `BookCatalog.IntegrationTests` | 16 | The API from the outside, over real HTTP, against real PostgreSQL. |

Each integration test truncates every table before it runs, so the tests do not depend on
each other or on the order they execute in.

---

## Migrations

The schema is created only by migrations, which are committed to the repository. A clone
gets a working schema from zero.

In `Development` the app applies pending migrations at startup, which is what makes
`docker compose up` work on a clean machine. This is deliberately gated to Development —
in production you do not want every starting instance racing to alter the schema.

To apply them by hand:

```bash
dotnet ef database update --project BookCatalog.Infrastructure --startup-project BookCatalog.Api
```

To add one:

```bash
dotnet ef migrations add <Name> --project BookCatalog.Infrastructure --startup-project BookCatalog.Api
```

---

## Project layout

```
BookCatalog.Domain          entities and their rules. references nothing.
BookCatalog.Application     services, DTOs, repository interfaces. references Domain.
BookCatalog.Infrastructure  EF Core, DbContext, repository implementations. references Application.
BookCatalog.Api             controllers, DI wiring, middleware. references both.
BookCatalog.Tests           unit tests.
BookCatalog.IntegrationTests  end-to-end tests via Testcontainers.
```

Dependencies point inward. `Domain` knows about nothing, `Application` knows about `Domain`,
and `Infrastructure` depends on the interfaces in `Application` rather than the other way
round — so the database is a detail the business logic never sees.

The dependency that matters is `Infrastructure → Application`. Because it points that way,
`Application` never references EF Core, so a database query cannot be written in a service —
the compiler stops it rather than a code review.

---

## Observability

**Health.** `/health/live` says the process is running and answers without touching
anything. `/health/ready` says the service can actually do its job — it checks the database,
and reports unhealthy when the database is down. Compose uses the liveness endpoint as the
container healthcheck.

**Logs** are JSON on stdout, which is where Docker collects them:

```bash
docker compose logs -f bookcatalog.api
```

Every log entry produced while handling a request carries a `TraceId`, and the same id comes
back to the caller in the `X-Trace-Id` response header. If someone reports a failed request,
that header finds every line it produced:

```bash
docker compose logs bookcatalog.api | grep <trace-id>
```

EF Core logs the SQL it generates, so you can see exactly what a query became. That is on in
Development and off in Production — `Microsoft.EntityFrameworkCore.Database.Command` is set
to `Information` in `appsettings.Development.json` and `Warning` in `appsettings.json`, so a
real deployment does not get a line per query.

---