using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NoteConnect.Data;
using NoteConnect.Models.Entities;

namespace NoteConnect.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ExternalLoginModel(
    SignInManager<User> signInManager,
    UserManager<User> userManager,
    ApplicationDbContext database) : PageModel
{
    public string ReturnUrl { get; private set; } = "/";
    public bool EmailAlreadyRegistered { get; private set; }

    public IActionResult OnGet() => RedirectToPage("./Login");

    public async Task<IActionResult> OnPostAsync(string provider, string? returnUrl = null)
    {
        SetReturnUrl(returnUrl);
        var providers = await signInManager.GetExternalAuthenticationSchemesAsync();
        if (!providers.Any(scheme => scheme.Name == provider))
            return BadRequest("Unknown sign-in provider.");

        var callback = Url.Page("./ExternalLogin", "Callback", new { returnUrl = ReturnUrl });
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, callback);
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
    {
        SetReturnUrl(returnUrl);
        if (remoteError is not null)
            return await ShowErrorAsync("Google sign-in was cancelled or could not be completed. Please try again.");

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
            return await ShowErrorAsync("Your sign-in session expired. Please start again from the login page.");

        // Preserve Identity's lockout, two-factor, and account-confirmation checks.
        var signIn = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: false);
        if (signIn.Succeeded)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return LocalRedirect(ReturnUrl);
        }
        if (signIn.RequiresTwoFactor)
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl, RememberMe = false });
        if (signIn.IsLockedOut)
            return RedirectToPage("./Lockout");
        if (signIn.IsNotAllowed)
            return await ShowErrorAsync("This account is not currently allowed to sign in. Check whether account confirmation is required.");

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
            return await ShowErrorAsync("Google did not provide a valid email address. Try another Google account or register with an email and password.");

        // Matching email addresses do not authorize linking an existing account.
        if (await userManager.FindByEmailAsync(email) is not null ||
            await userManager.FindByNameAsync(email) is not null)
        {
            EmailAlreadyRegistered = true;
            return await ShowErrorAsync("An account already uses this email address. Sign in to that account to link Google.");
        }

        var user = new User
        {
            UserName = email,
            Email = email,
            FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
            LastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty
        };

        // Create the user and provider link together so a failed link leaves no orphan account.
        await using (var transaction = await database.Database.BeginTransactionAsync())
        {
            var created = await userManager.CreateAsync(user);
            if (!created.Succeeded)
            {
                await transaction.RollbackAsync();
                return await ShowErrorsAsync(created);
            }

            var linked = await userManager.AddLoginAsync(user, info);
            if (!linked.Succeeded)
            {
                await transaction.RollbackAsync();
                return await ShowErrorsAsync(linked);
            }

            await transaction.CommitAsync();
        }

        // Use the external-login path again to apply the same policies to new accounts.
        var registeredSignIn = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: false);
        if (!registeredSignIn.Succeeded)
            return await ShowErrorAsync("Your account was created, but sign-in requires additional verification. Return to login to continue.");

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return LocalRedirect(ReturnUrl);
    }

    private void SetReturnUrl(string? returnUrl) =>
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Content("~/");

    private async Task<IActionResult> ShowErrorsAsync(IdentityResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return Page();
    }

    private async Task<IActionResult> ShowErrorAsync(string message)
    {
        ModelState.AddModelError(string.Empty, message);
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return Page();
    }
}
