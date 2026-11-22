# Mada Finishing backlog

Use the requirement IDs and numbered scenarios in [Discovery and Requirements](Mada_Finishing_01_Discovery_and_Requirements.md#client-acceptance-scenarios). The [Technical Design Draft](Mada_Finishing_02_Technical_Design_Draft.md#first-implementation-milestone) guides implementation and verification.

The .NET 11 MVC and MSTest scaffold is present.

| Order | Use case | Requirements and acceptance | Prerequisites | Status |
| --- | --- | --- | --- | --- |
| 1 | Manager inquiry intake and search | R1 (manager access), R2–R3, R4 (initial ownership only). Scenario 1, [first implementation milestone](Mada_Finishing_02_Technical_Design_Draft.md#first-implementation-milestone). | MVC and MSTest scaffold (present) | **Done** |
| 2 | Assign and reassign inquiries | R1 (coordinator access), R4, R7 (assignment history). Scenario 2. | Use case 1 **Done** | **Done** |
| 3 | Record coordinator follow-up and close an inquiry | R5–R7. Scenario 3. | Use case 2 **Done** | **Done** |

Mark an item **Ready** when the supervisor approves it for implementation and its prerequisites are complete. Follow the review and approval rules in [AGENTS.md](../AGENTS.md#backlog-workflow).

## First release closeout

Local release acceptance passed on November 22, 2026. The build completed with zero warnings and zero errors. All 12 MSTest checks passed, including five SQL Server integration tests, and the three browser acceptance scenarios passed. See [local handover](LOCAL_HANDOVER.md) for the evidence and operating instructions.

## Maintenance and next iteration

The maintenance backlog is open. For a reported defect, record the expected behavior, what happened, and the affected inquiry workflow. For a new capability, agree on the outcome and acceptance criteria before marking it Ready.

### Proposed use case 4. Review inquiries waiting for quotes

**Status:** Proposed. Supervisor approval is pending.

**Fictional client request:** Noor wants to see which inquiries are waiting for a quote so she can follow up with the coordinators during her weekly review.

**Missing business details and proposed decisions:** Treat an inquiry as waiting once it reaches Contacted and until it reaches Quote Sent or Closed. Use a live manager-only view for the weekly review, with counts by current coordinator and time elapsed since the inquiry entered Contacted. The existing status filter remains useful for finding individual records.

**Draft acceptance:** Noor sees only Contacted inquiries, with each inquiry's current coordinator and elapsed time since Contacted. Counts agree with the listed inquiries. A reassignment moves the inquiry to the new coordinator's group. Once the inquiry reaches Quote Sent or Closed it drops out. A coordinator cannot open the manager's report.
