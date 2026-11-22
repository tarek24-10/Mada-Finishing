# Milestones and backlog

## 5. Plan a milestone the client can try

The first release included a baseline and four milestones. Follow them in order. The requirement IDs and numbered demos refer to the [acceptance scenarios](Mada_Finishing_01_Discovery_and_Requirements.md#client-acceptance-scenarios).

- **0. Baseline — Done.** MVC solution, database configuration and MSTest project.
- **1. Intake — Done.** Noor can sign in, enter and find an inquiry. R1–R4, scenario 1.
- **2. Ownership — Done.** Noor can assign and reassign a saved inquiry. R1, R4 and R7, scenario 2.
- **3. Follow-up — Done.** The owner can record contact, notes and closure. R5–R7, scenario 3.
- **4. Local release and handover — Accepted.** See the [release record](LOCAL_HANDOVER.md#accepted-build).

## 8. Demo and review

Work on one use case at a time. Mark it **Ready** once its prerequisites are complete and the supervisor approves it. After the agreed checks pass, mark it **Ready for Review** and demonstrate its acceptance scenario. Mark it **Done** after supervisor approval. See the [working agreement](../AGENTS.md).

## Maintenance backlog

**Delivered after first acceptance.** Staff interface refresh, search results that update in place, and five-row inquiry pages. See [screen flow](Mada_Finishing_02_Technical_Design_Draft.md#screen-flow).

**Proposed: review inquiries waiting for quotes.** Noor wants a live view for her weekly review. Supervisor approval is pending.

- Include only Contacted inquiries. Show elapsed time since Contacted and counts by current coordinator.
- Reassignment moves the inquiry to its new owner's group. Quote Sent and Closed remove it from the report.
- Counts must match the rows shown. Only the manager can open the report.
