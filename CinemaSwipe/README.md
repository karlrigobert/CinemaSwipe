# 🎬 CinemaSwipe

A Tinder-style app for choosing what to watch at movie night — with your friends, in real time.

## How it works

1. **Create a room** — pick Movie or Series + genre. You get a 6-character code.
2. **Friends join** — they go to `/Room/Join` and enter the code (Sign in with Google).
3. **Swipe** — everyone gets the same 10 films one by one. Swipe right (♥) to like, left (✕) to pass.
4. **Winner** — when everyone finishes, the film with the most likes wins!

## Tech stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8 + Razor Pages (C#) |
| Database | SQL Server LocalDB / MSSQL |
| ORM | Entity Framework Core 8 |
| Real-time | SignalR |
| Auth | ASP.NET Identity + Google OAuth 2.0 |
| Film data | TMDB REST API |

---

## Setup instructions

### 1. Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server Express / LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (ships with Visual Studio)
- VS Code with the **C# Dev Kit** extension

### 2. Clone and open

```bash
git clone <your-repo>
cd CinemaSwipe
code .
```

### 3. Get a TMDB API key (free)

1. Sign up at https://www.themoviedb.org/
2. Go to **Settings → API** and request an API key (free, instant)
3. Copy your **API Key (v3 auth)**

### 4. Set up Google OAuth

1. Go to https://console.cloud.google.com/
2. Create a project → **APIs & Services → Credentials → Create OAuth 2.0 Client ID**
3. Application type: **Web application**
4. Authorised redirect URI: `https://localhost:5001/signin-google`
5. Copy **Client ID** and **Client Secret**

### 5. Configure secrets (never commit these!)

Use .NET user secrets:

```bash
dotnet user-secrets init
dotnet user-secrets set "Google:ClientId"     "YOUR_CLIENT_ID"
dotnet user-secrets set "Google:ClientSecret" "YOUR_CLIENT_SECRET"
dotnet user-secrets set "Tmdb:ApiKey"         "YOUR_TMDB_KEY"
```

Or edit `appsettings.json` locally (do not commit).

### 6. Run migrations & start

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

Then open https://localhost:5001

> The app also auto-migrates on startup (`db.Database.Migrate()` in Program.cs), so if you skip `dotnet ef database update`, the tables are still created when you first run.

---

## Project structure

```
CinemaSwipe/
├── Models/
│   └── Models.cs           # AppUser, Room, RoomMember, RoomFilm, Vote
├── Data/
│   └── AppDbContext.cs      # EF Core DbContext
├── Services/
│   ├── TmdbService.cs       # Fetch films from TMDB API
│   └── RoomService.cs       # Room creation, joining, voting, winner logic
├── Hubs/
│   └── RoomHub.cs           # SignalR hub for real-time events
├── Pages/
│   ├── Index.cshtml         # Landing page
│   ├── Auth/Login.cshtml    # Google sign-in
│   ├── Room/
│   │   ├── Create.cshtml    # Create room (pick type + genre)
│   │   ├── Join.cshtml      # Join with code
│   │   ├── Lobby.cshtml     # Waiting room (shows live members)
│   │   ├── Swipe.cshtml     # Tinder-style swipe UI
│   │   └── Result.cshtml    # Winner + vote breakdown
│   └── Shared/_Layout.cshtml
├── wwwroot/
│   └── css/app.css          # Full dark-mode stylesheet
├── schema.sql               # Reference SQL schema
└── Program.cs               # App bootstrap + DI setup
```

## Deploying online

For a publicly accessible URL, deploy to **Azure App Service** (free tier works):

```bash
az webapp up --name cinemaswipe --runtime "DOTNET:8.0" --sku F1
az webapp config connection-string set --name cinemaswipe \
  --settings Default="Server=tcp:..." --connection-string-type SQLAzure
```

Or use **Railway**, **Render**, or **Fly.io** — all support .NET 8 Docker containers. Add a `Dockerfile` using `mcr.microsoft.com/dotnet/aspnet:8.0` as the base image.

---

## Extending the app

- **Rematch** — allow the host to kick off a new round with different films
- **Custom film count** — let the host choose 5, 10, or 15 films
- **Poster swipe gesture** — already wired for touch; extend with mouse drag
- **Room history** — show past nights on the user's profile
