# Implementation design

## 6. Build one complete task

Use one .NET 11 ASP.NET Core MVC application with Razor views and Bootstrap RTL. Staff work through forms, lists and detail screens. Keep the separate MSTest project for unit and integration checks.

- **Request handling.** MVC controllers handle HTTP, form validation and view selection. ASP.NET Core Identity signs in staff and identifies their roles.
- **Business rules and storage.** Keep inquiry behavior on the inquiry model. The inquiry service uses EF Core directly for queries and writes. SQL Server stores the records. EF migrations update the schema.
- **Access and consistency.** Apply role and current-owner checks on the server for lists, detail URLs and form submissions. Save an ownership change and its history together. Reject stale updates so a failed change cannot add an activity entry.

## Screen flow

Sign-in opens the inquiry list. From the list, Noor can create an inquiry. Staff open a detail screen to read history and perform the actions their role allows.

Search and status changes return to page one. The list preserves both filters when paging, shows the matching count and returns five rows per page. EF Core counts filtered records and reads only the requested page, newest first. Search can update just the results. The design assumed roughly ten inquiries per business day.

## Logical data model

- **Staff user.** Identity account, display name and role. One coordinator can own several inquiries.
- **Inquiry.** Customer information, message, status, UTC creation time and optional current owner.
- **Inquiry activity.** Linked inquiry, actor, time and event details. Assignment events capture previous and new owners. Closure records the reason.

## 7. Test the behavior

- **Unit test.** Arrange an open inquiry. Act by closing it without a reason. Assert that it remains open. A failure points to the closure rule.
- **Integration test.** Arrange an isolated SQL Server database and signed-in staff sessions. Act through real MVC forms. Assert that access, saved records and history agree. Include invalid input, reassignment, concurrent writes and filtered paging.
- **Browser check.** Run the [three client scenarios](Mada_Finishing_01_Discovery_and_Requirements.md#client-acceptance-scenarios). Check Arabic layout, validation messages, keyboard use and desktop and mobile widths.

Run the [test commands](../README.md#test) before review. The original development environment used SDK 11.0.100 and ASP.NET Core runtime 11.0.0.
