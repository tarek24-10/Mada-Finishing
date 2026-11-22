# Mada Finishing local handover

## Accepted build

Local release acceptance passed on Windows on November 22, 2026. The tested commit was `86e63dd4eeb6dbd00c2ea0937c3f26708f05f1ca`. The merge commit on `main`, `64f71b049ad1144d965a70742b223850d0332f0c`, has the same Git file tree as the tested commit.

| Check | Result |
| --- | --- |
| Solution build | 0 warnings and 0 errors, as reported by the supervisor |
| MSTest | 12 passed, 0 failed, 0 skipped. All five SQL Server integration tests passed on LocalDB. |
| Intake and search | Noor created and found an unassigned inquiry by phone. A blank phone was rejected and saved no record. |
| Assignment and reassignment | Noor moved an inquiry from Mariam to Omar. Mariam lost list and direct-link access. Her saved link returned HTTP 404. Both assignments appeared in history. |
| Follow-up and closure | A blank closure reason left the inquiry at Quote Sent. A valid reason closed it. Omar retained read access, and Noor could see the full history. |

The automated counts were checked against the local MSTest report at `D:/MadaReleaseAcceptance-20261122-86e63dd/TestResults/release-acceptance.trx`. Browser results come from the supervisor's walkthrough. Screenshot capture failed during that walkthrough.

## Start and stop the dashboard

Use the Windows account that was configured for the local acceptance run. For first-time setup or a new Windows account, follow [Build and run](../README.md#build-and-run) and [Set up coordinators](../README.md#set-up-coordinators) in the web project README. Those instructions cover the .NET 11 SDK, SQL Server LocalDB, migrations, the development HTTPS certificate, and staff account setup.

After setup, open PowerShell in the repository root and run:

```powershell
dotnet run --project MadaFinishing.Web --launch-profile https
```

Open [the local dashboard](https://localhost:7015/). Press **Ctrl+C** in the terminal to stop the app. For a quick check after restarting, sign in as Noor and search for a known fictional inquiry by phone.

## Staff workflow

- **Noor, manager.** Enter inquiries received in the shared inbox, search by name or phone, filter by status, and assign or reassign open inquiries. Open an inquiry to read its history.
- **Sales coordinator.** Work from your assigned list. Open an inquiry to add notes, advance its status, and close it with a reason. Closed inquiries stay visible to the assigned coordinator and the manager.

## Data and maintenance

- The app reads its database connection from the web project's local configuration. Check which database it points to before applying migrations. The README's sample database name is an example.
- Keep staff passwords and connection details in local configuration. If keeping actual inquiry records, agree who will back up the configured database and verify a restore before relying on it.
- After changes, use the [test instructions](../README.md#test) with an isolated SQL Server test connection. Rerun the affected [acceptance scenarios](Mada_Finishing_01_Discovery_and_Requirements.md#client-acceptance-scenarios) and record the commit and results.
