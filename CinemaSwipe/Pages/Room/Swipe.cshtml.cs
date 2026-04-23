using CinemaSwipe.Data;
using CinemaSwipe.Hubs;
using CinemaSwipe.Models;
using CinemaSwipe.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CinemaSwipe.Pages.Room;

public class SwipeModel : PageModel
{
    private readonly RoomService _rooms;
    private readonly UserManager<AppUser> _users;
    private readonly AppDbContext _db;

    public SwipeModel(RoomService rooms, UserManager<AppUser> users, AppDbContext db)
    { _rooms = rooms; _users = users; _db = db; }

    public Models.Room? Room { get; private set; }
    public RoomFilm? CurrentFilm { get; private set; }
    public int VotedCount { get; private set; }
    public int TotalFilms { get; private set; } = 10;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _users.GetUserAsync(User);
        if (user is null) return Challenge();

        Room = await _rooms.GetRoomAsync(id);
        if (Room is null) return NotFound();
        if (Room.Status == RoomStatus.Done) return RedirectToPage("/Room/Result", new { id });

        var films = await _rooms.GetFilmsAsync(id);
        TotalFilms = films.Count;

        VotedCount = await _db.Votes
            .CountAsync(v => v.RoomFilm!.RoomId == id && v.UserId == user.Id);

        CurrentFilm = await _rooms.GetNextUnvotedFilmAsync(id, user.Id);
        return Page();
    }
}

public static class VoteEndpoint
{
    public record VoteRequest(int RoomId, int RoomFilmId, bool IsLike);

    public static void MapVoteEndpoint(WebApplication app) =>
        app.MapPost("/api/vote", async (
            [FromBody] VoteRequest req,
            UserManager<AppUser> users,
            RoomService rooms,
            IHubContext<RoomHub> hub,
            HttpContext ctx) =>
        {
            var user = await users.GetUserAsync(ctx.User);
            if (user is null) return Results.Unauthorized();

            var result = await rooms.CastVoteAsync(req.RoomId, user.Id, req.RoomFilmId, req.IsLike);
            if (!result.Ok) return Results.BadRequest();

            if (result.Done)
            {
                var room = await rooms.GetRoomAsync(req.RoomId);
                await RoomHubExtensions.NotifyRoomDone(hub, req.RoomId,
                    room?.WinnerTitle ?? "", room?.WinnerPoster ?? "");
            }
            else
            {
                await RoomHubExtensions.NotifyVoteCast(hub, req.RoomId, user.Id, 0, req.IsLike);
            }

            return Results.Ok(new { result.Done });
        }).RequireAuthorization();
}