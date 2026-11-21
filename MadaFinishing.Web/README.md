# Mada Finishing

Mada Finishing is an Arabic staff dashboard for entering and finding customer inquiries. The .NET 11 solution contains an ASP.NET Core MVC web project and an MSTest project.

## Build and run

Install the .NET 11 SDK and use a SQL Server instance. The following PowerShell 7 commands use Windows SQL Server LocalDB. Run them from the [repository root](../).

```powershell
dotnet build MadaFinishing.slnx

dotnet user-secrets set 'ConnectionStrings:MadaFinishing' 'Server=(localdb)\MSSQLLocalDB;Database=MadaFinishingLocalTest;Integrated Security=True;TrustServerCertificate=True' --project MadaFinishing.Web
dotnet user-secrets set 'InitialManager:Email' 'noor@example.test' --project MadaFinishing.Web
dotnet user-secrets set 'InitialManager:DisplayName' 'Noor' --project MadaFinishing.Web
$env:InitialManager__Password = Read-Host 'Initial manager password' -MaskInput
dotnet run --project MadaFinishing.Web --launch-profile https --no-build -- --setup-manager
Remove-Item Env:InitialManager__Password

dotnet dev-certs https --trust
dotnet run --project MadaFinishing.Web --launch-profile https
```

The development launch profile loads user secrets from your Windows account. The setup command applies migrations and creates the manager account and role. Use an Identity password of at least six characters with uppercase, lowercase, a digit, and a symbol. Rerunning setup keeps an existing account's password.

Open [the dashboard](https://localhost:7015/), sign in as Noor, enter a sample inquiry, and find it by phone. Confirm that it appears as **جديد** (New) and **غير مسند** (unassigned). Check a missing required field, Arabic right-to-left layout, and keyboard navigation at desktop and mobile widths. Press Ctrl+C to stop the app.

To test from the build output directory with the same user secrets, run `dotnet MadaFinishing.Web.dll --environment Development --urls http://localhost:5000`. For deployment, run `dotnet publish MadaFinishing.Web` and start the app from the publish directory.

## Test

```powershell
$env:MADA_TEST_SQL_SERVER = 'Server=(localdb)\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True'
dotnet test MadaFinishing.slnx --no-build
```

Each integration test creates a separate, uniquely named SQL Server database, applies migrations, provisions a demo manager, and deletes that database afterward. The SQL Server login must be able to create and delete databases. Without `MADA_TEST_SQL_SERVER`, integration tests are reported as skipped.

- **Inquiry unit test.** Arrange inquiry fields and a known creation time. Act by creating the inquiry. Assert that it records that time and starts New and unassigned. A failure indicates a problem with creation defaults.
- **Manager intake integration test.** Arrange a migrated test database, manager account, and older inquiries. Act by signing in, submitting the form, and searching. Assert that invalid input is rejected and valid input persists and appears in the matching results. A failure indicates which part of the request, validation, persistence, or list behavior broke.
