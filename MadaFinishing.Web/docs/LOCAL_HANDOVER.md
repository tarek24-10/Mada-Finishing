# Local release and handover

## Accepted build

**Step 9. Check the release.** The supervisor accepted the original local Windows release on November 22, 2026. The tested commit was `86e63dd4eeb6dbd00c2ea0937c3f26708f05f1ca`. Merge commit `64f71b049ad1144d965a70742b223850d0332f0c` had the same file tree.

- **Build.** The supervisor reported zero warnings and zero errors.
- **MSTest.** 12 passed, zero failed and zero skipped, including all five SQL Server integration tests on LocalDB. The local record is `D:/MadaReleaseAcceptance-20261122-86e63dd/TestResults/release-acceptance.trx`.
- **Browser acceptance.** The supervisor passed all [three scenarios](Mada_Finishing_01_Discovery_and_Requirements.md#client-acceptance-scenarios). Mariam's saved link returned HTTP 404 after reassignment, and a blank closure reason left the inquiry at Quote Sent. Screenshot capture failed during this walkthrough.

These results cover the original accepted version. Later changes are listed in the [maintenance backlog](BACKLOG.md#maintenance-backlog).

## Start and stop the dashboard

**Step 10. Hand over operation.** Use the configured Windows account. On a new machine or account, complete the [developer setup](../README.md#build-and-run), including coordinator setup. From the repository root, run:

```powershell
dotnet run --project MadaFinishing.Web --launch-profile https
```

Open [the local dashboard](https://localhost:7015/), sign in as Noor and search for a known sample inquiry by phone. Coordinators sign in with their own accounts to see their assignments. Press **Ctrl+C** in the terminal to stop.

## Close the engagement

**Step 11. Record acceptance.** The supervisor approved the local handover. Keep the tested commit, results and operator instructions together. For a paid engagement, send the final invoice or request the agreed milestone payment and confirm the support contact and dates.

## Support after delivery

**Step 12. Handle new requests.** For a defect, record who was signed in, the steps, the expected result and what happened. For a new feature, agree on the outcome and acceptance criteria before marking it Ready in the [backlog](BACKLOG.md).

Keep credentials in local configuration. Before storing real inquiries, assign responsibility for database backups and verify a restore. After changes, rerun the [relevant tests](../README.md#test) and affected client scenarios. Record the new commit and results.
