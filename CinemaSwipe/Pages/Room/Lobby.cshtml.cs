using CinemaSwipe.Hubs;
using CinemaSwipe.Models;
using CinemaSwipe.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace CinemaSwipe.Pages.Room;

public class LobbyModel : PageModel
{
    private readonly RoomService          _rooms;
    private readonly UserManager<AppUser> _users;
    private readonly IHubContext<RoomHub> _hub;

    public LobbyModel(RoomService rooms, UserManager<AppUser> users, IHubContext<RoomHub> hub)
    { _rooms = rooms; _users = users; _hub = hub; }

    public Models.Room? Room   { get; private set; }
    public bool         IsHost { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Room = await _rooms.GetRoomAsync(id);
        if (Room is null) return NotFound();
        if (Room.Status == RoomStatus.Done)     return RedirectToPage("/Room/Result", new { id });
        if (Room.Status == RoomStatus.Swiping)  return RedirectToPage("/Room/Swipe",  new { id });

        var user = await _users.GetUserAsync(User);
        IsHost = user?.Id == Room.HostId;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var user = await _users.GetUserAsync(User);
        if (user is null) return Challenge();

        bool ok = await _rooms.StartSwipingAsync(id, user.Id);
        if (!ok) return Forbid();

        await RoomHubExtensions.NotifySwipingStarted(_hub, id);
        return RedirectToPage("/Room/Swipe", new { id });
    }
}
