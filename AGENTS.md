# Repository Guidelines

## Project Structure & Module Organization

Devlivery is a delivery-management monorepo:

- `apps/api/src/Devlivery/`: .NET 10 API. Keep vertical slices in `Features`, business rules in `Domain`, adapters in `Infrastructure`, and shared plumbing in `Common`.
- `apps/api/src/Devlivery.BackupJob/`: PostgreSQL backup worker.
- `apps/api/test/Devlivery.Tests/`: backend tests, organized by feature and behavior.
- `apps/web/src/features/`: React/TypeScript features; `src/shared/` contains reusable components, hooks, and services. Static assets belong in `apps/web/public/`.
- `docs/`: architecture, configuration, development, and deployment guides.

## Build, Test, and Development Commands

Use .NET 10 SDK, Node.js 24, pnpm 10, and Docker. From `apps/api`:

```powershell
docker compose up -d postgres
dotnet tool restore
dotnet restore Devlivery.slnx
dotnet build Devlivery.slnx --no-restore
dotnet run --project src/Devlivery
```

These start PostgreSQL, restore tooling/dependencies, build, and launch the API. Before first launch, apply migrations with `dotnet ef database update --project src/Devlivery --context <Context>` for both `ApplicationDbContext` and `ApplicationIdentityDbContext`.

From `apps/web`, run `pnpm install --frozen-lockfile`, then `pnpm dev`. Use `pnpm lint` for Biome checks, `pnpm format` to rewrite formatting, and `pnpm build` for TypeScript validation and production output.

## Coding Style & Naming Conventions

Follow each application's `.editorconfig`. C# uses four spaces, PascalCase types/methods, `I`-prefixed interfaces, and file-scoped namespaces. Keep namespaces aligned with folders. Web code uses two spaces, double quotes, and Biome import organization; follow existing kebab-case filenames such as `use-date-range-filter.ts` and PascalCase React component names.

## Testing Guidelines

Backend tests use xUnit, Shouldly, NSubstitute, and PostgreSQL Testcontainers; keep Docker running for container-backed tests. Mirror feature folders, name classes `*Tests`, and use descriptive names such as `Handle_Should_Create_Product_With_Correct_Properties` with Arrange/Act/Assert sections.

From the repository root, run:

```powershell
dotnet test apps/api/Devlivery.slnx --no-restore --disable-build-servers -m:1 --verbosity minimal
```

Add regression tests for changed behavior. No numeric coverage threshold is configured in CI. The web package has no test script; run lint/build and manually verify affected flows.

## Commit & Pull Request Guidelines

Follow recent scoped commits: `fix(api): ...`, `feat(expenses): ...`, or `chore(ci): ...`. Keep changes focused. Describe behavior, validation, and related issues; include screenshots for UI changes. PRs targeting `main` must contain exactly one release marker—`[Patch]`, `[Minor]`, or `[Major]`—across the title and body.

## Security & Configuration

Keep credentials out of commits; use environment variables or .NET user secrets. Preserve tenant isolation and authenticated operator attribution. Consult `docs/configuration.md` before changing configuration.
