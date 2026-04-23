using Microsoft.AspNetCore.Identity;

namespace CinemaSwipe.Models;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = "";
    public string? AvatarUrl   { get; set; }
    public ICollection<RoomMember> RoomMemberships { get; set; } = new List<RoomMember>();
}

public class Room
{
    public int    Id         { get; set; }
    public string Code       { get; set; } = "";   // 6-char join code
    public string HostId     { get; set; } = "";
    public AppUser? Host     { get; set; }
    public string ContentType { get; set; } = "movie"; // "movie" | "series"
    public string Genre      { get; set; } = "";
    public RoomStatus Status { get; set; } = RoomStatus.Lobby;
    public int?   WinnerTmdbId { get; set; }
    public string? WinnerTitle  { get; set; }
    public string? WinnerPoster { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<RoomMember>  Members  { get; set; } = new List<RoomMember>();
    public ICollection<RoomFilm>    Films    { get; set; } = new List<RoomFilm>();
}

public enum RoomStatus { Lobby, Swiping, Done }

public class RoomMember
{
    public int     Id       { get; set; }
    public int     RoomId   { get; set; }
    public Room?   Room     { get; set; }
    public string  UserId   { get; set; } = "";
    public AppUser? User    { get; set; }
    public bool    FinishedSwiping { get; set; }
}

public class RoomFilm
{
    public int    Id       { get; set; }
    public int    RoomId   { get; set; }
    public Room?  Room     { get; set; }
    public int    TmdbId   { get; set; }
    public string Title    { get; set; } = "";
    public string PosterPath { get; set; } = "";
    public string Overview { get; set; } = "";
    public double Rating   { get; set; }
    public int    Position { get; set; } // 1-10 order
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}

public class Vote
{
    public int    Id         { get; set; }
    public int    RoomFilmId { get; set; }
    public RoomFilm? RoomFilm { get; set; }
    public string UserId     { get; set; } = "";
    public AppUser? User     { get; set; }
    public bool   IsLike     { get; set; } // true = swipe right
}
