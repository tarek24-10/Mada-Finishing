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
