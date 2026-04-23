using CinemaSwipe.Data;
using CinemaSwipe.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaSwipe.Services;

public class RoomService
{
    private readonly AppDbContext _db;
    private readonly TmdbService  _tmdb;
    private static readonly Random _rng = new();

    public RoomService(AppDbContext db, TmdbService tmdb)
    {
        _db   = db;
        _tmdb = tmdb;
    }

    // ── Create & Join ─────────────────────────────────────────────────────────

    public async Task<Room> CreateRoomAsync(string hostId, string contentType, string genre)
    {
        var code = GenerateCode();
        var room = new Room
        {
            Code        = code,
            HostId      = hostId,
            ContentType = contentType,
            Genre       = genre,
            Status      = RoomStatus.Lobby,
        };
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        // Add host as first member
        _db.RoomMembers.Add(new RoomMember { RoomId = room.Id, UserId = hostId });
        await _db.SaveChangesAsync();
        return room;
    }

    public async Task<(bool ok, string error, Room? room)> JoinRoomAsync(string userId, string code)
    {
        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Code == code.ToUpper());
        if (room is null) return (false, "Room not found.", null);
        if (room.Status != RoomStatus.Lobby) return (false, "Swiping already started.", null);

        bool already = await _db.RoomMembers.AnyAsync(m => m.RoomId == room.Id && m.UserId == userId);
        if (!already)
        {
            _db.RoomMembers.Add(new RoomMember { RoomId = room.Id, UserId = userId });
            await _db.SaveChangesAsync();
        }
        return (true, "", room);
    }

    // ── Start swiping ─────────────────────────────────────────────────────────

    public async Task<bool> StartSwipingAsync(int roomId, string requestingUserId)
    {
        var room = await _db.Rooms.FindAsync(roomId);
        if (room is null || room.HostId != requestingUserId) return false;
        if (room.Status != RoomStatus.Lobby) return false;

        // Fetch 10 films from TMDB
        var films = await _tmdb.GetRandomFilmsAsync(room.ContentType, room.Genre, 10);
        for (int i = 0; i < films.Count; i++)
        {
            _db.RoomFilms.Add(new RoomFilm
            {
                RoomId      = roomId,
                TmdbId      = films[i].TmdbId,
                Title       = films[i].Title,
                PosterPath  = films[i].PosterPath,
                Overview    = films[i].Overview,
                Rating      = films[i].Rating,
                Position    = i + 1,
            });
        }
        room.Status = RoomStatus.Swiping;
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Vote ──────────────────────────────────────────────────────────────────

    public async Task<VoteResult> CastVoteAsync(int roomId, string userId, int roomFilmId, bool isLike)
    {
        // Upsert vote
        var existing = await _db.Votes.FirstOrDefaultAsync(v => v.RoomFilmId == roomFilmId && v.UserId == userId);
        if (existing is null)
        {
            _db.Votes.Add(new Vote { RoomFilmId = roomFilmId, UserId = userId, IsLike = isLike });
        }
        else
        {
            existing.IsLike = isLike;
        }
        await _db.SaveChangesAsync();

        // Check if this user has voted on all 10 films
        int filmCount = await _db.RoomFilms.CountAsync(f => f.RoomId == roomId);
        int userVotes = await _db.Votes
            .CountAsync(v => v.RoomFilm!.RoomId == roomId && v.UserId == userId);

        if (userVotes >= filmCount)
        {
            var member = await _db.RoomMembers.FirstOrDefaultAsync(m => m.RoomId == roomId && m.UserId == userId);
            if (member is not null) { member.FinishedSwiping = true; await _db.SaveChangesAsync(); }
        }

        // Check if ALL members have finished
        int total    = await _db.RoomMembers.CountAsync(m => m.RoomId == roomId);
        int finished = await _db.RoomMembers.CountAsync(m => m.RoomId == roomId && m.FinishedSwiping);

        if (finished >= total)
        {
            await FinaliseRoomAsync(roomId);
            return new VoteResult(true, true);
        }
        return new VoteResult(true, false);
    }

    // ── Finalise ──────────────────────────────────────────────────────────────

    private async Task FinaliseRoomAsync(int roomId)
    {
        var films = await _db.RoomFilms
            .Include(f => f.Votes)
            .Where(f => f.RoomId == roomId)
            .ToListAsync();

        var winner = films
            .OrderByDescending(f => f.Votes.Count(v => v.IsLike))
            .ThenByDescending(f => f.Rating)
            .First();

        var room = await _db.Rooms.FindAsync(roomId);
        if (room is null) return;
        room.Status        = RoomStatus.Done;
        room.WinnerTmdbId  = winner.TmdbId;
        room.WinnerTitle   = winner.Title;
        room.WinnerPoster  = winner.PosterPath;
        await _db.SaveChangesAsync();
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<Room?> GetRoomAsync(int id) =>
        await _db.Rooms
            .Include(r => r.Members).ThenInclude(m => m.User)
            .Include(r => r.Films).ThenInclude(f => f.Votes)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Room?> GetRoomByCodeAsync(string code) =>
        await _db.Rooms.FirstOrDefaultAsync(r => r.Code == code.ToUpper());

    public async Task<List<RoomFilm>> GetFilmsAsync(int roomId) =>
        await _db.RoomFilms.Where(f => f.RoomId == roomId).OrderBy(f => f.Position).ToListAsync();

    public async Task<int> GetNextUnvotedPositionAsync(int roomId, string userId)
    {
        var voted = await _db.Votes
            .Where(v => v.RoomFilm!.RoomId == roomId && v.UserId == userId)
            .Select(v => v.RoomFilm!.Position)
            .ToListAsync();
        return voted.Count + 1; // next = first unvoted
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Range(0, 6).Select(_ => chars[_rng.Next(chars.Length)]).ToArray());
    }
    public async Task<RoomFilm?> GetNextUnvotedFilmAsync(int roomId, string userId)
    {
        var votedFilmIds = await _db.Votes
            .Where(v => v.RoomFilm!.RoomId == roomId && v.UserId == userId)
            .Select(v => v.RoomFilmId)
            .ToListAsync();

        return await _db.RoomFilms
            .Where(f => f.RoomId == roomId && !votedFilmIds.Contains(f.Id))
            .OrderBy(f => f.Position)
            .FirstOrDefaultAsync();
    }
}

public record VoteResult(bool Ok, bool Done);
