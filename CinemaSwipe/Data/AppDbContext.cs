using CinemaSwipe.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CinemaSwipe.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Room>       Rooms       => Set<Room>();
    public DbSet<RoomMember> RoomMembers => Set<RoomMember>();
    public DbSet<RoomFilm>   RoomFilms   => Set<RoomFilm>();
    public DbSet<Vote>       Votes       => Set<Vote>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Room>()
            .HasOne(r => r.Host)
            .WithMany()
            .HasForeignKey(r => r.HostId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<RoomMember>()
            .HasIndex(m => new { m.RoomId, m.UserId })
            .IsUnique();

        b.Entity<Vote>()
            .HasIndex(v => new { v.RoomFilmId, v.UserId })
            .IsUnique();

        // Prevent cascade cycles
        b.Entity<Vote>()
            .HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<RoomMember>()
            .HasOne(m => m.User)
            .WithMany(u => u.RoomMemberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
