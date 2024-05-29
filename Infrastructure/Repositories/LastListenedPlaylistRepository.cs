using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class LastListenedPlaylistRepository : ILastListenedPlaylistRepository
{
    private readonly PostgreDbContext _dbContext;
    private DbSet<LastListenedPlaylist> LastListenedPlaylists => _dbContext.Set<LastListenedPlaylist>();
    private ILogger<LastListenedPlaylistRepository> _logger;

    public LastListenedPlaylistRepository(PostgreDbContext dbContext, ILogger<LastListenedPlaylistRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<LastListenedPlaylist> GetById(Guid id, CancellationToken ct)
    {
        return LastListenedPlaylists.FirstAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyCollection<LastListenedPlaylist>> GetAll(CancellationToken ct)
    {
        return await LastListenedPlaylists.ToListAsync(ct);
    }

    public async Task Add(LastListenedPlaylist entity, CancellationToken ct)
    {
        var listenedTracks = await LastListenedPlaylists
            .Include(e => e.Playlist)
            .Where(lt => lt.User.Id == entity.User.Id)
            .OrderByDescending(lt => lt.ListenTime)
            .ToListAsync(ct);
        var existedListenedTrack = listenedTracks.Find(e => e.Playlist.Id == entity.Playlist.Id);
        if (existedListenedTrack != null)
            return;
        if (listenedTracks.Count >= 3)
        {
            var oldestListenedTrack = listenedTracks.Last();
            LastListenedPlaylists.Remove(oldestListenedTrack);
        }

        await LastListenedPlaylists.AddAsync(entity, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Update(LastListenedPlaylist entity, CancellationToken ct)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Delete(LastListenedPlaylist entity, CancellationToken ct)
    {
        LastListenedPlaylists.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<Playlist>> GetPlaylistsByUserId(Guid userId, CancellationToken ct)
    {
        return await LastListenedPlaylists
            .Include(e => e.Playlist.User)
            .Include(e=>e.Playlist.PlaylistTracks)
            .ThenInclude(e=>e.Track)
            .Where(e => e.User.Id == userId)
            .Select(e => e.Playlist)
            .ToListAsync(ct);
    }
}