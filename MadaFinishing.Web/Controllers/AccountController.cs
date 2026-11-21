using MadaFinishing.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MadaFinishing.Web.Controllers;

public sealed class AccountController(SignInManager<StaffUser> signInManager) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult SignIn(string? returnUrl)
    {
        return View(new SignInInput { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignIn(SignInInput input)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var result = await signInManager.PasswordSignInAsync(input.Email.Trim(), input.Password,
            isPersistent: false, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "تعذر تسجيل الدخول بهذه البيانات.");
            return View(input);
        }

        if (Url.IsLocalUrl(input.ReturnUrl))
        {
            return LocalRedirect(input.ReturnUrl);
        }

        return RedirectToAction(nameof(InquiriesController.Index), "Inquiries");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("SignOut")]
    public async Task<IActionResult> SignOutStaff()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(SignIn));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }
}
