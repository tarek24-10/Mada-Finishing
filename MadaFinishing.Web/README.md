# Developer setup and tests

## Build and run

For the approved local setup, install the .NET 11 SDK, SQL Server LocalDB and PowerShell 7. Run all commands from the repository root. Confirm the example database name before applying migrations.

```powershell
dotnet build MadaFinishing.slnx

dotnet user-secrets set 'ConnectionStrings:MadaFinishing' 'Server=(localdb)\MSSQLLocalDB;Database=MadaFinishingLocalTest;Integrated Security=True;TrustServerCertificate=True' --project MadaFinishing.Web
dotnet user-secrets set 'InitialManager:Email' 'noor@example.test' --project MadaFinishing.Web
dotnet user-secrets set 'InitialManager:DisplayName' 'Noor' --project MadaFinishing.Web
$env:InitialManager__Password = Read-Host 'Initial manager password' -MaskInput
dotnet run --project MadaFinishing.Web --launch-profile https --no-build -- --setup-manager
Remove-Item Env:InitialManager__Password
```

Setup applies migrations and creates the manager role and account. Use a password with at least six characters, uppercase, lowercase, a digit and a symbol. Repeating setup preserves an existing password. The development profile reads user secrets from the current Windows account.

```powershell
dotnet dev-certs https --trust
```

## Set up coordinators

Use a different email for each coordinator. Repeat for Omar. Each setup command also applies pending migrations.

```powershell
$env:InitialCoordinator__Email = 'mariam@example.test'
$env:InitialCoordinator__DisplayName = 'مريم'
$env:InitialCoordinator__Password = Read-Host 'Coordinator password' -MaskInput
dotnet run --project MadaFinishing.Web --launch-profile https --no-build -- --setup-coordinator
Remove-Item Env:InitialCoordinator__Email, Env:InitialCoordinator__DisplayName, Env:InitialCoordinator__Password
```

For daily startup, see the [operator instructions](docs/LOCAL_HANDOVER.md#start-and-stop-the-dashboard). To run directly from the build output with development secrets, use `dotnet MadaFinishing.Web.dll --environment Development --urls http://localhost:5000`. To create publish output, use `dotnet publish MadaFinishing.Web`.

## Test

Build first, then run the suite against an isolated test SQL Server instance. The login must be able to create and delete databases. Each integration test creates its own database, migrates it and removes it afterward. Without the environment variable below, SQL integration checks are skipped.

```powershell
$env:MADA_TEST_SQL_SERVER = 'Server=(localdb)\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True'
dotnet test MadaFinishing.slnx --no-build
```

To run a smaller group, append `--filter "FullyQualifiedName~ManagerCanReassign"` to the test command. See the [design](docs/Mada_Finishing_02_Technical_Design_Draft.md) for test coverage and the [release record](docs/LOCAL_HANDOVER.md#accepted-build) for historical results.
