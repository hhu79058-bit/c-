# Repository Guidelines

## Project Structure & Module Organization
- `Forms/`: WinForms UI forms and event handlers (e.g., `Forms/LoginForm.cs`).
- `Services/`: business logic and database access operations (e.g., `Services/OrderService.cs`).
- `Models/`: data models and status constants (e.g., `Models/Order.cs`).
- `Utils/`: shared utilities such as database helpers (`Utils/DBHelper.cs`).
- `Database/`: SQL scripts to create/update the schema.
- `App.config`: connection string and runtime configuration.
- `bin/`, `obj/`: build outputs (do not edit manually).

## Build, Test, and Development Commands
- Build (Visual Studio): open `WaiMai.sln` and build the solution.
- Build (MSBuild): `msbuild WaiMai.csproj /p:Configuration=Debug`.
- Run locally: start the project in Visual Studio, or run `bin\Debug\WaiMai.exe` after a successful build.
- Database setup: run `Database/CreateDatabase.sql` against SQL Server, then apply `Database/UpdatePaymentTable.sql` if needed.

## Coding Style & Naming Conventions
- Language: C# (.NET Framework 4.8), WinForms.
- Indentation: 4 spaces, brace-on-new-line style (follow existing files).
- Naming: PascalCase for classes/methods/properties; camelCase for locals/parameters.
- UI files: use `*Form.cs` naming, with paired `.Designer.cs` and `.resx` files.

## Testing Guidelines
- No test framework is configured in this repository.
- If you add tests, place them in a new `Tests/` directory and document how to run them.

## Commit & Pull Request Guidelines
- No Git history exists yet, so there are no established commit message conventions.
- Suggested commit format: `type: short summary` (e.g., `feat: add order filters`).
- PRs should include a brief description, steps to verify, and screenshots for UI changes.

## Security & Configuration Tips
- Connection strings live in `App.config`. Do not commit credentials.
- Default database name is `WaiMaiSystem` on local SQL Server (`Data Source=.`).
