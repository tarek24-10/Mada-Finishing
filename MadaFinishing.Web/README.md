# Mada Finishing

Mada Finishing is an Arabic staff dashboard for entering and finding customer inquiries. The .NET 11 solution contains an ASP.NET Core MVC web project and an MSTest project.

See the [local handover](docs/LOCAL_HANDOVER.md) for the accepted build and day-to-day operation.

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

### Set up coordinators

Run this setup once per coordinator. Use a separate email address for each staff account. Setup applies any pending migrations and keeps the password of an existing account.

```powershell
$env:InitialCoordinator__Email = 'mariam@example.test'
$env:InitialCoordinator__DisplayName = 'مريم'
$env:InitialCoordinator__Password = Read-Host 'Coordinator password' -MaskInput
dotnet run --project MadaFinishing.Web --launch-profile https --no-build -- --setup-coordinator
Remove-Item Env:InitialCoordinator__Email, Env:InitialCoordinator__DisplayName, Env:InitialCoordinator__Password
```

Repeat for Omar. Sign in as Noor, open an inquiry from the list, and assign it to Mariam. In a separate browser session, sign in as Mariam, open it, and save its URL. As Noor, reassign it to Omar and verify both entries in the assignment history. Confirm that Mariam cannot find or open it and that Omar can.

### Check follow-up and closure

Run either staff setup command to apply the latest migration to an existing development database. As Omar, open the assigned inquiry, mark it Contacted, add a call note, and mark it Quote Sent. Try closing it with a blank reason and verify that it remains Quote Sent. Close it with a reason, then sign in as Noor. Verify the owner and the assignment, note, status, and closure entries. Both Noor and Omar can read the closed inquiry.

## Test

```powershell
$env:MADA_TEST_SQL_SERVER = 'Server=(localdb)\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True'
dotnet test MadaFinishing.slnx --no-build
```

Each integration test creates a separate, uniquely named SQL Server database, applies migrations, provisions a demo manager, and deletes that database afterward. The SQL Server login must be able to create and delete databases. Without `MADA_TEST_SQL_SERVER`, integration tests are reported as skipped.

- **Inquiry unit test.** Arrange inquiry fields and a known creation time. Act by creating the inquiry. Assert that it records that time and starts New and unassigned. A failure indicates a problem with creation defaults.
- **Manager intake integration test.** Arrange a migrated test database, manager account, and older inquiries. Act by signing in, submitting the form, and searching. Assert that invalid input is rejected and valid input persists and appears in the matching results. A failure indicates which part of the request, validation, persistence, or list behavior broke.
- **Assignment tests.** The unit test arranges an inquiry assigned to Mariam, reassigns it to Omar, and asserts that status stays the same and history records both owners. The HTTP integration test arranges a migrated database and separate signed-in sessions for Noor, Mariam, and Omar, then checks access and history after assigning and reassigning. The concurrency test loads the same inquiry twice, saves one assignment, then asserts that the stale save fails and only the first entry persists. To run these checks, add `--filter "FullyQualifiedName~AssignTo|FullyQualifiedName~ManagerCanReassign|FullyQualifiedName~ConcurrentAssignment"` to the test command above.
- **Follow-up and closure tests.** The unit tests arrange an inquiry in each open status, attempt closure with and without a reason, and assert that valid closure records the reason and blocks later changes. The HTTP integration test arranges an inquiry with a current and former coordinator, acts through the follow-up forms, and asserts that only the current coordinator can write and that the detail page and SQL Server agree after closure. To run these checks, add `--filter "FullyQualifiedName~ChangeStatus_RequiresReason|FullyQualifiedName~AddNote_Rejects|FullyQualifiedName~AssignedCoordinatorRecordsFollowUp"` to the test command above.
