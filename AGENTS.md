# Repository Guidelines

## Project Structure & Module Organization

Devlivery is a delivery-management monorepo:

- `apps/api/src/Devlivery/`: .NET 10 API. Keep vertical slices in `Features`, business rules in `Domain`, adapters in `Infrastructure`, and shared plumbing in `Common`.
- `apps/api/test/Devlivery.Tests/`: backend tests, organized by feature and behavior.
- `apps/backup/`: standalone PostgreSQL backup job (`src/Devlivery.BackupJob/`, tests in `test/Devlivery.BackupJob.Tests/`). It shares no code with the API; keep it that way so it can be removed when the hosting provides managed backups.
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

Backend tests use xUnit, Shouldly, NSubstitute, and PostgreSQL Testcontainers; keep Docker running for container-backed tests. Mirror feature folders, name classes `*Tests`, and name tests `GivenContext_WhenAction_ThenOutcome` (e.g. `GivenClosedSession_WhenAddingPayment_ThenThrowsDomainException`) with separate Arrange/Act/Assert sections. Build test data with `Common/Builders`; HTTP contract tests live in `Features/<Feature>/Api` and use `ApiFactory`. See `docs/development/testing-strategy.md`.

From the repository root, run:

```powershell
dotnet test apps/api/Devlivery.slnx --no-restore --disable-build-servers -m:1 --verbosity minimal
dotnet test apps/backup/Devlivery.BackupJob.slnx --disable-build-servers -m:1 --verbosity minimal
```

Add regression tests for changed behavior. No numeric coverage threshold is configured in CI. Frontend tests use Vitest, Testing Library, and MSW. Keep them next to the corresponding source files: `.test.ts` without JSX and `.test.tsx` with JSX; do not use `.spec.*`. Shared test infrastructure belongs in `apps/web/test/`, imported via `@test/*`; future end-to-end tests belong in `apps/web/test/e2e/`. Run `pnpm test`, lint/build, and manually verify affected flows.

## Commit & Pull Request Guidelines

Follow recent scoped commits: `fix(api): ...`, `feat(expenses): ...`, or `chore(ci): ...`. Keep changes focused. Describe behavior, validation, and related issues; include screenshots for UI changes. PRs targeting `main` must contain exactly one release marker—`[Patch]`, `[Minor]`, or `[Major]`—across the title and body.

## Security & Configuration

Keep credentials out of commits; use environment variables or .NET user secrets. Preserve tenant isolation and authenticated operator attribution. Consult `docs/development/configuration.md` before changing configuration.
