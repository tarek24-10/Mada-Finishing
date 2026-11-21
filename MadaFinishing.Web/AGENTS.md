# Mada Finishing project guidance

- Work on one agreed use case at a time and keep changes within its scope. Read the relevant requirements and technical design in `docs/` before editing.
- Before implementation, explain where request handling, business rules, and data access will live within the approved web project. Ask the supervisor to approve any technical choice that has not been agreed, including architecture, dependencies, and code organization conventions.
- If a business requirement is missing, make a realistic fictional-client decision and document both the gap and the assumption.
- Use clear business names, focused methods, and straightforward control flow. Keep controllers focused on HTTP request handling and views. Keep inquiry rules together.
- Apply SOLID principles where they solve a specific problem. Explain the need before proposing a new abstraction. Preserve the approved direct EF Core approach.
- For database changes, check for unnecessary data loading and repeated queries. Use expected volume or measurements to justify pagination, indexes, or infrastructure changes.
- Build and run the relevant checks. Use MSTest for approved automated tests. For the first example of each test type and when introducing an unfamiliar test, explain Arrange–Act–Assert and how to run the test.
- Review the diff against the acceptance criteria before handing back the work. Report which checks ran, what passed, any failures, and what remains unverified.
- Ask before pushing or deploying.

## Backlog workflow

- When planning work, read [the requirements](docs/Mada_Finishing_01_Discovery_and_Requirements.md) and use [the technical design draft](docs/Mada_Finishing_02_Technical_Design_Draft.md) as the implementation guide. Consult [the backlog](docs/BACKLOG.md) to select a use case.
- “Implement the next ready use case” means select the first supervisor-approved use case in backlog order marked **Ready** whose prerequisites are complete. Implement only that use case and follow the rules above. If none qualifies, report which approval or prerequisite is missing.
- Mark the use case **Ready for Review** after the agreed checks pass. Mark it **Done** only after the supervisor approves the result.
