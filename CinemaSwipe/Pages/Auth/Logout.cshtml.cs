using CinemaSwipe.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CinemaSwipe.Pages.Auth;

public class LogoutModel : PageModel
{
    private readonly SignInManager<AppUser> _signIn;

    public LogoutModel(SignInManager<AppUser> signIn)
    { _signIn = signIn; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        await _signIn.SignOutAsync();
        return RedirectToPage("/Index");
    }
}