# Mada Finishing — Stage 3 design draft

## Purpose

Translate the approved workflow into screens and data needs, then confirm the technical choices before implementation. Scope and acceptance scenarios are maintained in the [discovery and requirements document](Mada_Finishing_01_Discovery_and_Requirements.md).

## Approved

| Decision | Choice | Reason |
|---|---|---|
| Source repository | Use the existing Mada Finishing Git repository | It contains the project history and solution scaffold. |
| Web application | .NET 11 and ASP.NET Core MVC with Razor views | The staff workflow uses forms, tables, filters, and a detail screen. |
| Database | SQL Server with EF Core and migrations | The data is relational. An inquiry change and its associated history can be saved together. |
| Data access organization | Direct EF Core access in focused inquiry code | Inquiry rules stay separate from MVC controllers within the web project. |
| Staff sign-in | ASP.NET Core Identity | Initial staff accounts are provisioned during setup. Roles identify managers and coordinators, and current assignment determines each coordinator's access. |
| UI styling | Bootstrap RTL | Staff screens use Arabic forms, tables, and navigation. |
| Project structure | One MVC web project and one separate test project | Unit and integration tests are grouped separately inside the test project. Inquiry rules stay separate from controllers within the MVC project. |
| Test framework | MSTest | Both unit and integration tests use the framework the supervisor has already used. |

## Screen flow

| Screen | Behavior |
|---|---|
| Sign-in | Staff sign in, then land on the inquiry list. |
| Inquiry list | The manager sees all inquiries. A coordinator sees their own assigned inquiries. Staff can search by name or phone and filter by status. Show newest first with customer, service, date entered, status, and owner. |
| New inquiry | The manager enters a customer name, phone, service type, and optional message. The inquiry starts as New and unassigned. |
| Inquiry details | Show the customer message, current owner, status, and ordered history. The manager can assign or reassign an open inquiry. Its current coordinator can add notes, change status, or close it with a reason. Closed inquiries are read-only. |

**Inquiry list pagination (R3).** The brief did not specify list size or daily volume. For the fictional client, assume about 10 inquiries per business day. The list shows 20 per page as the manager's records accumulate. A search or status change starts on page one. Paging preserves both filters and displays the total matching count. EF Core counts the filtered rows and reads only the requested page in newest-first order. No schema change is needed.

Staff screens are in Arabic and use a right-to-left layout. The server applies the same access rules to list results, direct detail URLs, and form submissions.

## Logical data model

| Record | Data and relationship |
|---|---|
| Staff user | Identity account and display name. A user acts as a manager or coordinator for this workflow. |
| Inquiry | Customer name, phone, service type, optional customer message, creation time, status, and optional current coordinator. A coordinator can own multiple inquiries. |
| Inquiry activity | Belongs to one inquiry and references the staff member who acted. Stores the event time and details appropriate to an assignment, status change, or note. An assignment records the old and new owner where applicable. Closure records its reason. |

**Missing business detail.** The client brief did not define duplicate phone numbers. For this simulation, the same customer can make separate inquiries for different jobs. Search returns all matching inquiries within the signed-in user's access.

**Design check.** If Noor reassigns an inquiry from Mariam to Omar, the new owner and history entry are saved together. Omar can open the inquiry, and Mariam loses access to it from both the list and a saved link.

## First-release test plan

| Test type | What it verifies |
|---|---|
| Unit | Inquiry rules in isolation. Assigning an inquiry leaves the status unchanged, closing requires a reason, and closed inquiries cannot be edited or reassigned. |
| Integration | The running MVC application, signed-in staff, and a dedicated SQL Server test database. Verify that reassignment changes list and detail visibility, a former owner cannot submit a stale update, and successful changes persist with the corresponding history. A rejected update leaves the inquiry and history unchanged. |
| Manual client acceptance | Complete intake, reassignment, follow-up, and closure with demo accounts. Check Arabic RTL layout, validation messages, keyboard navigation, and the inquiry list and forms on desktop and mobile. |

Begin with unit tests, a small set of integration scenarios, and a repeatable manual acceptance checklist. Browser automation can be reconsidered if repeated UI regressions make manual checks costly.

For the first test of each type, walk through the requirement, Arrange–Act–Assert, how the test runs, and what a failure means. For integration tests, also explain how demo users are set up and how test data is isolated from application data.

## First implementation milestone

The manager signs in, enters an inquiry, and finds it in the list as New and unassigned. This covers staff sign-in, a SQL Server migration, the first form, server-side validation, and a persisted record. Assignment and coordinator follow-up are the next milestones.

## Implementation environment

The implementation environment has .NET SDK 11.0.100 and ASP.NET Core runtime 11.0.0.
