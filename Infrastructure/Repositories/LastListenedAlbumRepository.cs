using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class LastListenedAlbumRepository : ILastListenedAlbumRepository
{
    private readonly PostgreDbContext _dbContext;
    private DbSet<LastListenedAlbum> LastListenedAlbums => _dbContext.Set<LastListenedAlbum>();
    private ILogger<LastListenedAlbumRepository> _logger;

    public LastListenedAlbumRepository(PostgreDbContext dbContext, ILogger<LastListenedAlbumRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<LastListenedAlbum> GetById(Guid id, CancellationToken ct)
    {
        return LastListenedAlbums.FirstAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyCollection<LastListenedAlbum>> GetAll(CancellationToken ct)
    {
        return await LastListenedAlbums.ToListAsync(ct);
    }

    public async Task Add(LastListenedAlbum entity, CancellationToken ct)
    {
        var listenedTracks = await LastListenedAlbums
            .Include(e => e.Album)
            .Where(lt => lt.User.Id == entity.User.Id)
            .OrderByDescending(lt => lt.ListenTime)
            .ToListAsync(ct);
        var existedListenedTrack = listenedTracks.Find(e => e.Album.Id == entity.Album.Id);
        if (existedListenedTrack != null)
            return;
        if (listenedTracks.Count >= 3)
        {
            var oldestListenedTrack = listenedTracks.Last();
            LastListenedAlbums.Remove(oldestListenedTrack);
        }

        await LastListenedAlbums.AddAsync(entity, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Update(LastListenedAlbum entity, CancellationToken ct)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Delete(LastListenedAlbum entity, CancellationToken ct)
    {
        LastListenedAlbums.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<Album>> GetAlbumsByUserId(Guid userId, CancellationToken ct)
    {
        return await LastListenedAlbums
            .Include(e => e.Album.Tracks)
            .Include(e => e.Album.User)
            .Where(e => e.User.Id == userId)
            .Select(e => e.Album)
            .ToListAsync(ct);
    }
}