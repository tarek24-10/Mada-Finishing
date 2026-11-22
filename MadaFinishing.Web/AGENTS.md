# Mada Finishing project guidance

- Work on one agreed use case at a time and keep changes within its scope. Read the relevant requirements and technical design in `docs/` before editing.
- Before implementation, explain where request handling, business rules, and data access will live within the approved web project. Ask the supervisor to approve any technical choice that has not been agreed, including architecture, dependencies, and code organization conventions.
- If a business requirement is missing, make a realistic fictional-client decision and document both the gap and the assumption.
- Use clear business names, focused methods, and straightforward control flow. Keep controllers focused on HTTP request handling and views. Keep inquiry rules together.
- Apply SOLID principles where they solve a specific problem. Explain the need before proposing a new abstraction. Mada uses EF Core directly in the inquiry service. In other projects, follow the existing persistence design. Introduce a repository when it serves a concrete need, such as sharing aggregate loading and saving across workflows.
- For database changes, check for unnecessary data loading and repeated queries. Use expected volume or measurements to justify pagination, indexes, or infrastructure changes.
- Choose checks from the verification table below. Use MSTest for approved automated tests. For the first example of each test type and when introducing an unfamiliar test, explain Arrange–Act–Assert and how to run the test.
- Review the diff against the acceptance criteria before handing back the work. Report which checks ran, what passed, any failures, and what remains unverified.
- The supervisor owns pushes and merges. Deployment requires separate approval.

## Refactoring workflow for MVC projects

1. Trace a user action from its Razor form through the controller to the business rule and database query. Identify the specific name, method or screen that makes the flow difficult to follow.
2. Keep persisted entities in `Models` and screen data and form inputs in `ViewModels`. Name methods for what the caller needs, such as `SearchAsync` and `GetDetailsAsync`. Update callers, Razor imports and validation bindings in the same change.
3. Extract a method or Razor partial when it describes a distinct task, such as loading or displaying inquiry history. Preserve authorization, query ordering, concurrency handling and form values on validation errors.
   - Give a repeated business rule one implementation and call it from each workflow that uses it. Keep EF Core filters composable so filtering runs before counting, sorting and paging.
   - Share rendering when multiple screens display the same value. For example, the list, details and history views all use `_CairoTime` for the localized timestamp and its machine-readable value.
   - In JavaScript, separate fetching results from updating the DOM. Preserve keyboard focus, announcements, browser history and edits made while a request is in flight.
4. Put the primary task first on each screen. Use the installed Bootstrap grid, forms, cards and buttons. Keep custom CSS for branding and behavior the framework does not cover. Check keyboard focus, validation, RTL text and narrow layouts.
5. Inspect the changed files, including new files, against the acceptance criteria.

## Local work and manual review

1. Use two long-lived local branches: `main` for reviewed work and `work` for implementation. Start with `git status --short --branch`. On a clean, synchronized checkout, use `git switch work` (first time: `git switch -c work main`), then `git merge --ff-only main` on `work`. Preserve existing edits or pending reviews if the branches diverge.
2. Work on one reviewable change at a time. Read the relevant requirement, then search for the entry point and direct callers. Expand the search only when a dependency or failure warrants it. Reuse findings and summarize large outputs.
3. Commit the relevant files locally on `work`. Hand off the commit, what changed, checks run and remaining risks. The supervisor reviews `git diff main...work` and controls integration.
4. For a GitHub PR, the supervisor pushes `work` (first push: `git push -u origin work`), opens a PR with base `main` and head `work`, and merges it manually. Use a merge commit to keep the reusable branch history connected. Sync with `git switch main`, `git pull --ff-only origin main`, `git switch work`, then `git merge --ff-only main`.
5. If the supervisor wants to merge locally first, use `git switch main` and `git merge --ff-only work` after review. Push only `work` before opening the PR. Pushing the merged `main` first would publish the change and leave the PR with nothing to review.

## Verification by change

| Change | Smallest useful check |
| --- | --- |
| Documentation only | Read the diff and check changed paths or commands. Skip the build and application tests. |
| CSS, Bootstrap or Razor | Check the affected screen and responsive layout. Build once for Razor or binding changes. |
| JavaScript interaction | Check syntax and exercise the changed interaction, including focus or history when affected. |
| C# rule, bug fix or refactor | Build once and run the MSTest cases covering the affected behavior. |
| EF Core query, migration, authorization or concurrency | Run the affected integration scenario on the documented test database. Check the relevant access or data boundary. |
| Release or broad change with uncertain impact | Run the full suite once and the relevant acceptance scenarios. |

Run a baseline check when existing behavior is uncertain. Repeat a check after a relevant edit or environment fix. Report blocked and skipped checks explicitly.

## Choosing a model

| Task | Suggested starting point |
| --- | --- |
| Wording, Git commands, mechanical edits | GPT-6 Luna, medium |
| MVC features, Bootstrap UI, focused refactors, unit tests and routine debugging | GPT-6.2 Sol, medium |
| Interactive desktop or browser walkthrough | GPT-6.1 Terra, medium |
| Architecture trade-offs, migration design, concurrency bugs or complex code review | GPT-6.2 Astra, medium. Use high effort when important uncertainty persists. |

Choose once per task from the models available. Escalate for a specific unresolved question and pass along the relevant evidence.

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
