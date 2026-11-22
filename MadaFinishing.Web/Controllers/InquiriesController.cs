using MadaFinishing.Web.Inquiries;
using MadaFinishing.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MadaFinishing.Web.Controllers;

[Authorize(Roles = StaffUser.ManagerRole + "," + StaffUser.CoordinatorRole)]
public sealed class InquiriesController(InquiryService inquiries, UserManager<StaffUser> users) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, InquiryStatus? status, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var staffId = users.GetUserId(User);
        if (staffId is null)
        {
            return Forbid();
        }

        var rows = await inquiries.ListAsync(staffId, User.IsInRole(StaffUser.ManagerRole), search, status, cancellationToken);
        return View(new InquiryListViewModel(search, status, rows));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var staffId = users.GetUserId(User);
        if (staffId is null)
        {
            return Forbid();
        }

        var details = await inquiries.DetailsAsync(id, staffId, User.IsInRole(StaffUser.ManagerRole), cancellationToken);
        return details is null ? NotFound() : View(details);
    }

    [HttpGet]
    [Authorize(Roles = StaffUser.ManagerRole)]
    public IActionResult Create() => View(new InquiryInput());

    [HttpPost]
    [Authorize(Roles = StaffUser.ManagerRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InquiryInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        await inquiries.CreateAsync(input, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = StaffUser.ManagerRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int id, string? coordinatorId, CancellationToken cancellationToken)
    {
        var managerId = users.GetUserId(User);
        if (managerId is null)
        {
            return Forbid();
        }

        var result = await inquiries.AssignAsync(id, coordinatorId, managerId, cancellationToken);
        if (result is AssignmentResult.Assigned or AssignmentResult.Unchanged)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        if (result == AssignmentResult.InquiryNotFound)
        {
            return NotFound();
        }

        var details = await inquiries.DetailsAsync(id, managerId, isManager: true, cancellationToken);
        if (details is null)
        {
            return NotFound();
        }

        if (result == AssignmentResult.Conflict)
        {
            Response.StatusCode = StatusCodes.Status409Conflict;
        }

        ModelState.AddModelError(string.Empty, result switch
        {
            AssignmentResult.InquiryClosed => "لا يمكن تغيير مسؤول استفسار مغلق.",
            AssignmentResult.InvalidCoordinator => "اختر منسق مبيعات صالحًا.",
            _ => "تغير الاستفسار أثناء الحفظ. راجع المسؤول الحالي وحاول مرة أخرى."
        });
        return View(nameof(Details), details);
    }

    [HttpPost]
    [Authorize(Roles = StaffUser.CoordinatorRole)]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> AddNote(int id, string? note, CancellationToken cancellationToken) =>
        RunFollowUpAsync(id, staffId => inquiries.AddNoteAsync(id, staffId, note, cancellationToken),
            note, null, cancellationToken);

    [HttpPost]
    [Authorize(Roles = StaffUser.CoordinatorRole)]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> ChangeStatus(
        int id, InquiryStatus? status, string? reason, CancellationToken cancellationToken) =>
        RunFollowUpAsync(id, staffId => inquiries.ChangeStatusAsync(id, staffId, status, reason, cancellationToken),
            null, reason, cancellationToken);

    private async Task<IActionResult> RunFollowUpAsync(
        int id, Func<string, Task<FollowUpResult>> save, string? note, string? reason, CancellationToken cancellationToken)
    {
        var staffId = users.GetUserId(User);
        if (staffId is null || User.IsInRole(StaffUser.ManagerRole))
        {
            return Forbid();
        }

        var result = await save(staffId);
        if (result == FollowUpResult.Saved)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        if (result == FollowUpResult.InquiryNotFound)
        {
            return NotFound();
        }

        var details = await inquiries.DetailsAsync(id, staffId, isManager: false, cancellationToken);
        if (details is null)
        {
            return NotFound();
        }

        if (result == FollowUpResult.Conflict)
        {
            Response.StatusCode = StatusCodes.Status409Conflict;
        }

        ModelState.AddModelError(string.Empty, result switch
        {
            FollowUpResult.InquiryClosed => "الاستفسار مغلق ولا يمكن تعديله.",
            FollowUpResult.InvalidNote => $"أدخل ملاحظة لا تتجاوز {InquiryFollowUp.NoteMaxLength} حرفًا.",
            FollowUpResult.InvalidStatus => "اختر الحالة التالية المتاحة.",
            FollowUpResult.InvalidClosureReason => $"أدخل سبب إغلاق لا يتجاوز {InquiryFollowUp.ClosureReasonMaxLength} حرفًا.",
            _ => "تغير الاستفسار أثناء الحفظ. راجع الحالة الحالية وحاول مرة أخرى."
        });
        return View(nameof(Details), details with { Note = note, ClosureReason = reason });
    }
}
