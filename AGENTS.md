# Project: Insurance Management Platform

## Agreed Technology Stack
- Frontend: React with TypeScript
- Backend: ASP.NET Core Web API (.NET / C#)
- Database: PostgreSQL
- Authentication: JWT

## Architecture
- Follow Clean Architecture, keeping domain and application concerns independent of infrastructure and presentation.
- Keep controllers thin; put business logic in application services.
- Use dependency injection for application and infrastructure services.
- Use Entity Framework Core for database access.
- Use asynchronous APIs (`async`/`await`) for I/O operations.
- Use DTOs for API requests and responses; do not expose persistence entities directly.

## Coding Standards
- Follow SOLID principles.
- Keep code readable, cohesive, and maintainable.
- Validate all incoming requests and return safe, useful error responses.
- Use functional React components and TypeScript.
- Build reusable frontend components and keep API calls separate from UI components.

## Testing
- Write unit tests for business logic.
- Use xUnit for .NET tests.
- Run existing tests and relevant builds before completing changes; report any failures.

## Security
- Never hardcode secrets. Load secrets from environment variables, user secrets, or a managed secret store.
- Enforce role-based authorization on protected API operations.
- Validate that the current user has permission to perform each operation.
- Never expose passwords, tokens, signing keys, or other sensitive information in API responses or logs.