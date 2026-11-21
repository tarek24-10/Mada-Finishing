# Mada Finishing Customer Inquiry Dashboard

## Stage 1. Discovery

**General step.** Confirm the users, the existing workflow, where it fails, and how the client will judge improvement.

**Project.** Mada Finishing is a fictional residential finishing company in Mansoura. Noor manages three sales coordinators. Website inquiries arrive in a shared inbox. Staff copy them into a spreadsheet and assign them through WhatsApp. Ownership gets lost, customers receive duplicate calls, and Noor cannot easily see which inquiries are waiting for a quote.

**Goal.** Noor can find an inquiry and tell who owns it, what its current status is, and what happened last. Coordinators can work from a list of their assigned inquiries. The dashboard is for staff.

## Stage 2. Requirements and scope

**General step.** Agree on the first release, define business rules and permissions, and write scenarios the client can use to accept the work.

### First release

| ID | Requirement |
|---|---|
| R1 | Staff sign in as a manager or a sales coordinator. The manager sees all inquiries. Coordinators see only inquiries currently assigned to them, including when opening a direct link. |
| R2 | The manager enters inquiries received in the shared inbox. Capture customer name, phone number, service type, and an optional message. The system records when each inquiry was created. |
| R3 | Staff can browse the newest inquiries first, search by customer name or phone, and filter by status within their own access. Each row shows the customer, service, date, status, and current owner. |
| R4 | The manager can assign or reassign an open inquiry to one coordinator. New inquiries start unassigned and assignment does not change their status. |
| R5 | The assigned coordinator can add notes or change the status of an open inquiry. The manager can read the details and history. |
| R6 | The usual status flow is New → Contacted → Quote Sent → Closed. The assigned coordinator can close an inquiry from any open status and must give a reason. Closed inquiries remain available for viewing. |
| R7 | The details screen shows the inquiry and the activity history. The history records assignments and reassignments, notes, and status changes with the actor and time. A closure entry includes its reason. |

**Later requests.** Automatic website intake, sending quotes from the dashboard, WhatsApp integration, notifications, reporting, and staff account management.

### Missing requirements and fictional client decisions

| Missing detail | Decision for this project |
|---|---|
| How do new inquiries arrive? | Noor enters them from the shared inbox. Seed data is available for demos. This makes intake part of the first release. |
| Can Noor update status or add notes herself? | Noor manages assignment. The assigned coordinator records follow-up. |
| What happens when an inquiry closes? | It stays visible to the manager and the last assigned coordinator. Both can read the history. Reopening, reassignment, and editing are unavailable after closure. |
| What does the team mean by Quote Sent? | Staff send quotes through their existing process and record the result in the dashboard. |
| What language do staff need? | Use an Arabic staff interface with right-to-left layout. Technical documentation is in English for the exercise. |
| How are staff accounts created? | The initial manager and coordinator accounts are provisioned during setup. In-app account administration can be scoped later. |
| Is there a service catalogue or a required phone format? | During intake, Noor enters the service type as text and records the phone as supplied. The system trims surrounding whitespace and searches the recorded phone by substring. |
| Which time zone should the inquiry list use? | Show the date and time in Cairo. Store the creation timestamp in UTC. |
| What happens when Noor selects the current coordinator again or another assignment is saved at the same time? | Selecting the current coordinator leaves ownership and history unchanged. If another change is saved while this request is in progress, show the latest details and ask Noor to review them before trying again. |

### Client acceptance scenarios

1. **Intake and search (R2–R3).** Noor enters an inquiry from an email, finds it by phone number, and sees it as New and unassigned.
2. **Reassignment (R1, R4, R7).** Noor assigns the inquiry to Mariam, then reassigns it to Omar. Omar can open it. Mariam loses access through the list and a saved direct link. Noor can see both assignments in the history.
3. **Follow-up and closure (R5–R7).** Omar marks the inquiry Contacted, adds a call note, and marks it Quote Sent. Closing without a reason leaves it at Quote Sent. Closing with a reason saves the change. Noor can see the owner, notes, status changes, and closing reason. Omar can still view the closed record.

## Stage 3. Design

Technical decisions are reserved for supervisor approval. The first choices concern the starting codebase, technology baseline, and application architecture. Once agreed, the design records the approved structure and the minimum wireframes, data model, and API contract needed to build the workflow.
