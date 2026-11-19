# Cards API — learning C# and ASP.NET Core

> **📚 Learning project — not production code.**
> I built this step by step while learning C# / .NET (coming from Java/Spring Boot),
> following a guided tutorial with [Claude](https://claude.ai) as my tutor. The code is mine, typed and debugged
> by hand, but the structure and exercises came from the tutorial. It exists to practise the platform,
> not to be a finished product.

A small REST API for a trading-card collection.

## What I practised

- Minimal-API endpoints in ASP.NET Core, OpenAPI docs (Scalar)
- EF Core with SQLite — migrations, configuration, SQL logging
- Options pattern with validation (`DataAnnotations`)
- JWT bearer authentication and role-based authorization (`dotnet user-jwts` for dev tokens)
- Integration tests with xUnit and `WebApplicationFactory`
- Structured logging and OpenTelemetry (viewed in the Aspire dashboard)
- Container images via `dotnet publish` (no Dockerfile)
- A final refactor into layers: `Domain`, `Application` (ports), `Infrastructure`, `Api` — hexagonal-style

## Stack

.NET 10 · ASP.NET Core · EF Core · SQLite · xUnit · OpenTelemetry · just

## Running it

```bash
just tools        # restore dotnet-ef
just db-update    # create the dev database
just run          # API with hot reload
just test
```

Run `just` to see all recipes (database, auth tokens, container, telemetry).

---

[Constantin Theis](https://portfolio.ths.lu)
