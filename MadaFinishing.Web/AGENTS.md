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

## Refactoring workflow for MVC projects

1. Trace a user action from its Razor form through the controller to the business rule and database query. Identify the specific name, method or screen that makes the flow difficult to follow.
2. Keep persisted entities in `Models` and screen data and form inputs in `ViewModels`. Name methods for what the caller needs, such as `SearchAsync` and `GetDetailsAsync`. Update callers, Razor imports and validation bindings in the same change.
3. Extract a method or Razor partial when it describes a distinct task, such as loading or displaying inquiry history. Preserve authorization, query ordering, concurrency handling and form values on validation errors.
   - Give a repeated business rule one implementation and call it from each workflow that uses it. Keep EF Core filters composable so filtering runs before counting, sorting and paging.
   - Share rendering when multiple screens display the same value. For example, the list, details and history views all use `_CairoTime` for the localized timestamp and its machine-readable value.
   - In JavaScript, separate fetching results from updating the DOM. Preserve keyboard focus, announcements, browser history and edits made while a request is in flight.
4. Put the primary task first on each screen. Use the installed Bootstrap grid, forms, cards and buttons. Keep custom CSS for branding and behavior the framework does not cover. Check keyboard focus, validation, RTL text and narrow layouts.
5. Run the following from the solution root before and after a refactor. Review new files listed by Git as well as the tracked diff. Use the project's documented test database for integration checks and report skipped tests separately.

```powershell
git status --short
dotnet build
dotnet test --no-build
git diff --check
git diff --stat
```

## Backlog workflow

- When planning work, read [the requirements](docs/Mada_Finishing_01_Discovery_and_Requirements.md) and use [the technical design draft](docs/Mada_Finishing_02_Technical_Design_Draft.md) as the implementation guide. Consult [the backlog](docs/BACKLOG.md) to select a use case.
- “Implement the next ready use case” means select the first supervisor-approved use case in backlog order marked **Ready** whose prerequisites are complete. Implement only that use case and follow the rules above. If none qualifies, report which approval or prerequisite is missing.
- Mark the use case **Ready for Review** after the agreed checks pass. Mark it **Done** only after the supervisor approves the result.
