using CinemaSwipe.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CinemaSwipe.Models;

namespace CinemaSwipe.Pages.Room;

public class CreateModel : PageModel
{
    private readonly RoomService    _rooms;
    private readonly UserManager<AppUser> _users;

    public CreateModel(RoomService rooms, UserManager<AppUser> users)
    { _rooms = rooms; _users = users; }

    [BindProperty] public string ContentType { get; set; } = "movie";
    [BindProperty] public string Genre       { get; set; } = "Action";

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _users.GetUserAsync(User);
        if (user is null) return Challenge();

        var room = await _rooms.CreateRoomAsync(user.Id, ContentType, Genre);
        return RedirectToPage("/Room/Lobby", new { id = room.Id });
    }
}
