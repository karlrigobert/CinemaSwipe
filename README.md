🎬 CinemaSwipe
A Tinder-style web app for choosing what to watch at movie night with friends. Everyone swipes through the same films simultaneously, and the film with the most likes wins.
---
How it works
Create a room — the host picks Movie or Series and a genre, and gets a 6-character room code
Friends join — each person opens the app, enters the code, and signs in with Google
Swipe — everyone gets the same 10 random films one by one. Swipe ♥ to like or ✕ to pass
Winner — when everyone finishes, the film with the most likes is announced
---
Tech stack
Layer	Technology
Language	C# (.NET 10)
Framework	ASP.NET Core Razor Pages
Database	SQL Server (LocalDB / Azure SQL)
ORM	Entity Framework Core 10
Real-time	SignalR (live lobby + winner announcement)
Authentication	ASP.NET Identity + Google OAuth 2.0
Film data	TMDB REST API
Frontend	HTML, CSS, Vanilla JavaScript
---
Project structure
```
CinemaSwipe/
├── Models/
│   └── Models.cs           # AppUser, Room, RoomMember, RoomFilm, Vote
├── Data/
│   └── AppDbContext.cs      # EF Core DbContext + relationships
├── Services/
│   ├── TmdbService.cs       # Fetches films from TMDB API
│   └── RoomService.cs       # Room creation, joining, voting, winner logic
├── Hubs/
│   └── RoomHub.cs           # SignalR hub for real-time events
├── Pages/
│   ├── Index.cshtml         # Landing page
│   ├── Auth/
│   │   ├── Login.cshtml     # Google sign-in page
│   │   └── Logout.cshtml    # Sign out
│   └── Room/
│       ├── Create.cshtml    # Create a room
│       ├── Join.cshtml      # Join with a code
│       ├── Lobby.cshtml     # Waiting room (live member list)
│       ├── Swipe.cshtml     # Tinder-style swipe UI
│       └── Result.cshtml    # Winner + vote breakdown
├── wwwroot/
│   └── css/app.css          # Full dark-mode stylesheet
├── Program.cs               # App bootstrap + dependency injection
└── schema.sql               # Reference SQL schema
```
---
Setup instructions
Prerequisites
.NET 10 SDK
SQL Server Express / LocalDB
Visual Studio Code with the C# Dev Kit extension
1. Get a TMDB API key (free)
The app uses The Movie Database (TMDB) to fetch real film and series data.
Sign up at https://www.themoviedb.org/signup
Go to Settings → API and request an API key (free, approved instantly)
Copy your API Key (v3 auth)
2. Set up Google OAuth (free)
The app uses Google Sign-In for authentication.
Go to https://console.cloud.google.com
Create a new project called `CinemaSwipe`
Go to APIs & Services → OAuth consent screen → choose External → fill in app name and email
Go to APIs & Services → Credentials → Create OAuth 2.0 Client ID
Application type: Web application
Add this to Authorised redirect URIs:
```
   http://localhost:5000/signin-google
   ```
Copy the Client ID and Client Secret
3. Configure secrets
In the project folder, run these commands one at a time:
```bash
dotnet user-secrets init
dotnet user-secrets set "Google:ClientId" "YOUR_GOOGLE_CLIENT_ID"
dotnet user-secrets set "Google:ClientSecret" "YOUR_GOOGLE_CLIENT_SECRET"
dotnet user-secrets set "Tmdb:ApiKey" "YOUR_TMDB_API_KEY"
```
4. Run the app
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```
Then open http://localhost:5000 in your browser.
> **Note:** If `dotnet ef` is not found, install it first:
> ```bash
> dotnet tool install --global dotnet-ef
> ```
---
Testing with two users
To test the full swiping flow locally:
Open the app in Chrome and sign in with one Google account
Open the app in Edge (or a Chrome Incognito window) and sign in with a different Google account
Create a room in one browser, join with the code in the other
Both users swipe through all 10 films and the winner is announced automatically
---
Key features
Real-time lobby — members appear instantly as they join using SignalR
Google Sign-In — no passwords, uses existing Google accounts
Random film selection — 10 films fetched live from TMDB based on chosen genre
Touch support — swipe gestures work on mobile devices
Vote tallying — most likes wins, ties broken by TMDB rating
Full vote breakdown — results page shows how many likes each film received
Dark mode UI — clean dark theme works on desktop and mobile
---
API keys notice
The API keys (Google OAuth and TMDB) are stored using .NET User Secrets and are not included in the project files for security reasons. Anyone running the app needs to generate their own free keys following the steps above. Both APIs are completely free for personal/educational use.
