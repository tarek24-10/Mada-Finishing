using MadaFinishing.Web.Inquiries;
using MadaFinishing.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MadaFinishing.Web.Controllers;

[Authorize(Roles = StaffUser.ManagerRole)]
public sealed class InquiriesController(InquiryService inquiries) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, InquiryStatus? status, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var rows = await inquiries.ListAsync(search, status, cancellationToken);
        return View(new InquiryListViewModel(search, status, rows));
    }

    [HttpGet]
    public IActionResult Create() => View(new InquiryInput());

    [HttpPost]
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
}
