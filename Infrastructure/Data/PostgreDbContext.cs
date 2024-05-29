using Domain.Entities;
using Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class PostgreDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<Album> Albums { get; set; }
    public DbSet<LastListenedTrack> LastListenedTracks { get; set; }
    public DbSet<LastListenedAlbum> LastListenedAlbums { get; set; }
    public DbSet<LastListenedPlaylist> LastListenedPlaylists { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Transaction> Transactions { get; set; }


    public PostgreDbContext(DbContextOptions<PostgreDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        BuildUserModel(modelBuilder);
    }

    private static void BuildUserModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(e => e.Role)
            .HasConversion(
                v => v.ToString(),
                v => (Roles)Enum.Parse(typeof(Roles), v));
        modelBuilder.Entity<User>()
            .Property(e => e.Status)
            .HasConversion(v => v.ToString(),
                v => (Statuses)Enum.Parse(typeof(Statuses), v));
    }
}