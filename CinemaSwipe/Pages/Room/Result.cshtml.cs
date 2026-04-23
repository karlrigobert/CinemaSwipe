using CinemaSwipe.Data;
using CinemaSwipe.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CinemaSwipe.Pages.Room;

public class FilmResult
{
    public int    TmdbId     { get; set; }
    public string Title      { get; set; } = "";
    public string PosterPath { get; set; } = "";
    public int    Likes      { get; set; }
}

public class ResultModel : PageModel
{
    private readonly AppDbContext         _db;
    private readonly UserManager<AppUser> _users;

    public ResultModel(AppDbContext db, UserManager<AppUser> users)
    { _db = db; _users = users; }

    public Models.Room?      Room  { get; private set; }
    public List<FilmResult>  Films { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Room = await _db.Rooms.FindAsync(id);
        if (Room is null) return NotFound();

        Films = await _db.RoomFilms
            .Where(f => f.RoomId == id)
            .Select(f => new FilmResult
            {
                TmdbId     = f.TmdbId,
                Title      = f.Title,
                PosterPath = f.PosterPath,
                Likes      = f.Votes.Count(v => v.IsLike),
            })
            .OrderByDescending(f => f.Likes)
            .ToListAsync();

        return Page();
    }
}
