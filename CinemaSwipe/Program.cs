using CinemaSwipe.Data;
using CinemaSwipe.Hubs;
using CinemaSwipe.Models;
using CinemaSwipe.Pages.Room;
using CinemaSwipe.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// ── Identity ──────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    opt.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── Google OAuth ──────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultScheme = IdentityConstants.ApplicationScheme;
    opt.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddGoogle(opt =>
{
    opt.ClientId = builder.Configuration["Google:ClientId"]!;
    opt.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
    opt.CallbackPath = "/signin-google";
    opt.Scope.Add("profile");
    opt.Scope.Add("email");
});

// ── App services ───────────────────────────────────────────────────────────────
builder.Services.AddHttpClient<TmdbService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddSignalR();
builder.Services.AddAntiforgery();
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Auth/Login";
    opt.Cookie.SameSite = SameSiteMode.Lax;
    opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddDataProtection();
builder.Services.AddRazorPages(opt =>
{
    opt.Conventions.AuthorizePage("/Room/Create");
    opt.Conventions.AuthorizePage("/Room/Lobby");
    opt.Conventions.AuthorizePage("/Room/Swipe");
    opt.Conventions.AuthorizePage("/Room/Result");
});

var app = builder.Build();

// ── Migrate on startup ─────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapHub<RoomHub>("/hubs/room");
VoteEndpoint.MapVoteEndpoint(app);

app.Run();
