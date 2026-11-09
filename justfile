# Cards API: day-to-day commands
# Run `just` (no arguments) to list everything.

api   := "src/Cards.Api"
infra := "src/Cards.Infrastructure"
db    := ".data/cards.db"

# EF commands run from inside the Api folder, so the relative SQLite path
# (Data Source=../../.data/cards.db) resolves exactly like it does at runtime.
ef := "cd " + api + " && dotnet ef --project ../Cards.Infrastructure --startup-project ."

# List all recipes
default:
    @just --list --unsorted

# ─── Dev ──────────────────────────────────────────────────────────────

# Run the API with hot reload
[group('dev')]
run:
    dotnet watch --project {{api}}

# Run with EF's SQL logging switched on
[group('dev')]
run-sql:
    Logging__LogLevel__Microsoft.EntityFrameworkCore.Database.Command=Information dotnet watch --project {{api}}

# Build the whole solution
[group('dev')]
build:
    dotnet build

# Run all tests, or only those matching a filter, e.g. `just test Delete`
[group('dev')]
test filter="":
    {{ if filter == "" { "dotnet test" } else { "dotnet test --filter " + filter } }}

# Format code according to .editorconfig / defaults
[group('dev')]
fmt:
    dotnet format

# Show outdated NuGet packages
[group('dev')]
outdated:
    dotnet list package --outdated

# Restore local tools (dotnet-ef) after cloning
[group('dev')]
tools:
    dotnet tool restore

# ─── Database (EF Core) ───────────────────────────────────────────────

# Create a new migration, e.g. `just db-add AddDecks`
[group('db')]
db-add name:
    {{ef}} migrations add {{name}}

# Remove the LAST migration (only if it is not applied yet)
[group('db')]
db-remove:
    {{ef}} migrations remove

# List migrations and whether they are applied
[group('db')]
db-list:
    {{ef}} migrations list

# Apply migrations, or migrate to a target, e.g. `just db-update InitialCreate` (`0` = undo all)
[group('db')]
db-update target="":
    {{ef}} database update {{target}}

# Does the model differ from the last migration? (run before committing)
[group('db')]
db-check:
    {{ef}} migrations has-pending-model-changes

# Print the SQL for all migrations (idempotent: safe to run on any DB state)
[group('db')]
db-script:
    {{ef}} migrations script --idempotent

# Delete the dev database and rebuild it from migrations (with seed data)
[group('db')]
[confirm("Delete the dev database and recreate it?")]
db-reset:
    rm -f {{db}} {{db}}-shm {{db}}-wal
    {{ef}} database update

# Open the dev database in the sqlite3 shell
[group('db')]
db-shell:
    sqlite3 {{db}}

# ─── Auth ─────────────────────────────────────────────────────────────

# Print a dev JWT for a normal user
[group('auth')]
token-user name="alice":
    @dotnet user-jwts create --project {{api}} --name {{name}} --output token

# Print a dev JWT with the admin role
[group('auth')]
token-admin name="bob":
    @dotnet user-jwts create --project {{api}} --name {{name}} --role admin --output token

# ─── Containers & telemetry ───────────────────────────────────────────

# Start the container runtime
[group('ops')]
colima:
    colima start

# Start the Aspire dashboard (UI: http://localhost:18888, OTLP: localhost:4317)
[group('ops')]
dashboard:
    docker run --rm -it -p 18888:18888 -p 4317:18889 -e DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true --name aspire-dashboard mcr.microsoft.com/dotnet/aspire-dashboard:latest

# Build the container image (no Dockerfile needed)
[group('ops')]
image:
    dotnet publish {{api}} -c Release -t:PublishContainer

# Run the container image on http://localhost:8080
[group('ops')]
container:
    docker run --rm -p 8080:8080 -e ConnectionStrings__Cards="Data Source=/home/app/cards.db" -e Database__MigrateOnStartup=true -v cards-data:/home/app --name cards-api cards-api:latest
