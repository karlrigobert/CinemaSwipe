using System.Text.Json;
using CinemaSwipe.Models;

namespace CinemaSwipe.Services;

public class TmdbService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private static readonly Random _rng = new();

    // Common TMDB genre IDs
    public static readonly Dictionary<string, int> GenreIds = new()
    {
        ["Action"]      = 28,
        ["Comedy"]      = 35,
        ["Drama"]       = 18,
        ["Horror"]      = 27,
        ["Romance"]     = 10749,
        ["Sci-Fi"]      = 878,
        ["Thriller"]    = 53,
        ["Animation"]   = 16,
        ["Documentary"] = 99,
        ["Fantasy"]     = 14,
    };

    public TmdbService(HttpClient http, IConfiguration cfg)
    {
        _http   = http;
        _apiKey = cfg["Tmdb:ApiKey"] ?? throw new InvalidOperationException("Tmdb:ApiKey missing");
        _http.BaseAddress = new Uri("https://api.themoviedb.org/3/");
    }

    public async Task<List<TmdbFilm>> GetRandomFilmsAsync(string contentType, string genre, int count = 10)
    {
        bool isMovie = contentType == "movie";
        string endpoint = isMovie ? "discover/movie" : "discover/tv";
        int genreId = GenreIds.TryGetValue(genre, out var gid) ? gid : 28;

        // Fetch 3 pages, merge, shuffle, take count
        var all = new List<TmdbFilm>();
        int page = _rng.Next(1, 8);
        for (int i = 0; i < 3 && all.Count < count * 3; i++)
        {
            var url = $"{endpoint}?api_key={_apiKey}&with_genres={genreId}&sort_by=popularity.desc" +
                      $"&vote_count.gte=100&page={page + i}&language=en-US";
            var json = await _http.GetStringAsync(url);
            var doc  = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("results", out var results))
            {
                foreach (var item in results.EnumerateArray())
                {
                    var film = ParseFilm(item, isMovie);
                    if (film is not null && film.PosterPath.Length > 0)
                        all.Add(film);
                }
            }
        }
        return all.OrderBy(_ => _rng.Next()).Take(count).ToList();
    }

    private TmdbFilm? ParseFilm(JsonElement el, bool isMovie)
    {
        try
        {
            return new TmdbFilm
            {
                TmdbId     = el.GetProperty("id").GetInt32(),
                Title      = isMovie
                    ? el.GetProperty("title").GetString() ?? ""
                    : el.GetProperty("name").GetString()  ?? "",
                PosterPath = el.TryGetProperty("poster_path", out var p) && p.ValueKind != JsonValueKind.Null
                    ? p.GetString() ?? "" : "",
                Overview   = el.TryGetProperty("overview", out var ov) ? ov.GetString() ?? "" : "",
                Rating     = el.TryGetProperty("vote_average", out var r) ? r.GetDouble() : 0,
            };
        }
        catch { return null; }
    }
}

public record TmdbFilm(
    int    TmdbId,
    string Title,
    string PosterPath,
    string Overview,
    double Rating)
{
    public TmdbFilm() : this(0, "", "", "", 0) { }
    public int    TmdbId     { get; init; } = TmdbId;
    public string Title      { get; init; } = Title;
    public string PosterPath { get; init; } = PosterPath;
    public string Overview   { get; init; } = Overview;
    public double Rating     { get; init; } = Rating;
    public string PosterUrl  => PosterPath.Length > 0
        ? $"https://image.tmdb.org/t/p/w500{PosterPath}" : "/img/no-poster.svg";
}
