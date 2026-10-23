# CoverOS

Agency-first insurance operations for construction contractors. React + TypeScript, ASP.NET Core 8, PostgreSQL/EF Core, JWT, and Clean Architecture.

The first review checkpoint includes agency registration, sign-in/sign-out, an authenticated Overview, and a read-only Company page backed by PostgreSQL. See [implementation checkpoints and acceptance tests](docs/IMPLEMENTATION_PLAN.md). Later features wait for owner testing of this checkpoint.

## Run locally

Prerequisites: .NET 8 SDK, Node.js/npm, Docker Compose (or an existing PostgreSQL 16 server).

1. Copy `.env.example` to `.env`, fill in a strong `POSTGRES_PASSWORD`, then run `docker compose up -d`. This file configures Compose only; .NET configuration is set separately below.
2. In your API terminal, configure the connection and generate a signing key. Substitute the same database password you set in `.env`:

   ```bash
   export ConnectionStrings__CoverOS='Host=localhost;Port=5432;Database=coveros;Username=coveros;Password=YOUR_LOCAL_DATABASE_PASSWORD'
   export Jwt__SigningKey="$(openssl rand -base64 48)"
   ```

   Keep these values out of source control. Use a dedicated empty development database for the first migration. An existing PostgreSQL installation can use its own host, port and credentials instead of Compose.
3. Apply the versioned schema, then start the API in the same terminal:

   ```bash
   dotnet run --project backend/src/CoverOS.Api -- --migrate
   dotnet run --project backend/src/CoverOS.Api
   ```

   The API listens on `http://localhost:5170`; Development Swagger is at `/swagger`.
4. In another terminal:

   ```bash
   cd frontend
   npm ci
   npm run dev
   ```

   Open `http://localhost:5173`, select **Create an agency**, and create your own account. There are no seeded users or shared passwords. Set `VITE_API_BASE_URL` if using another API origin.

JWTs expire after 30 minutes and are held only in browser memory. Reloading the page requires signing in again. Sign-out clears the browser token; issued tokens remain valid until expiry. The API checks active membership when loading account details. Signing keys must be at least 32 bytes. Passwords use ASP.NET Core's salted password hasher; responses contain account DTOs only.

## Checks

```bash
dotnet build CoverOS.sln -m:1
dotnet test CoverOS.sln -m:1
cd frontend
npm run build
npm run lint
```

`-m:1` avoids parallel MSBuild worker issues in restricted environments. The xUnit runner requires local socket access.

## Layout

- `backend/src/CoverOS.Domain` — account and organization entities
- `backend/src/CoverOS.Application` — DTOs, application service and infrastructure interfaces
- `backend/src/CoverOS.Infrastructure` — EF Core/PostgreSQL persistence, migrations, password hashing
- `backend/src/CoverOS.Api` — thin controllers, JWT issuance/validation, request validation and rate limiting
- `backend/tests/CoverOS.Application.Tests` — xUnit business logic tests
- `frontend/src/api` — HTTP operations separate from React UI

Follow [AGENTS.md](AGENTS.md) for coding and security standards.
