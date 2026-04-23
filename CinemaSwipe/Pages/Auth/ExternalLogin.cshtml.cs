using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CinemaSwipe.Models;

namespace CinemaSwipe.Pages.Auth;

public class ExternalLoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signIn;
    private readonly UserManager<AppUser> _users;

    public ExternalLoginModel(SignInManager<AppUser> signIn, UserManager<AppUser> users)
    { _signIn = signIn; _users = users; }

    public IActionResult OnPost(string provider, string returnUrl = "/")
    {
        var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
        var properties = _signIn.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = "/")
    {
        var info = await _signIn.GetExternalLoginInfoAsync();
        if (info is null) return RedirectToPage("./Login");

        var result = await _signIn.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true);
        if (result.Succeeded) return LocalRedirect(returnUrl);

        // First time — create the account
        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
        var name = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? email;
        var avatar = info.Principal.FindFirst("urn:google:picture")?.Value;

        var user = new AppUser { UserName = email, Email = email, DisplayName = name, AvatarUrl = avatar };
        var createResult = await _users.CreateAsync(user);
        if (createResult.Succeeded)
        {
            await _users.AddLoginAsync(user, info);
            await _signIn.SignInAsync(user, isPersistent: true);
            return LocalRedirect(returnUrl);
        }
        return RedirectToPage("./Login");
    }
}