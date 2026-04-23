using CinemaSwipe.Hubs;
using CinemaSwipe.Models;
using CinemaSwipe.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace CinemaSwipe.Pages.Room;

public class JoinModel : PageModel
{
    private readonly RoomService          _rooms;
    private readonly UserManager<AppUser> _users;
    private readonly IHubContext<RoomHub> _hub;

    public JoinModel(RoomService rooms, UserManager<AppUser> users, IHubContext<RoomHub> hub)
    { _rooms = rooms; _users = users; _hub = hub; }

    [BindProperty] public string Code { get; set; } = "";
    public string ErrorMessage { get; private set; } = "";

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _users.GetUserAsync(User);
        if (user is null) return Challenge();

        var (ok, error, room) = await _rooms.JoinRoomAsync(user.Id, Code);
        if (!ok) { ErrorMessage = error; return Page(); }

        await RoomHubExtensions.NotifyMemberJoined(_hub, room!.Id, user.DisplayName);
        return RedirectToPage("/Room/Lobby", new { id = room.Id });
    }
}
