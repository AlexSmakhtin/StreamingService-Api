using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class LastListenedTrackRepository : ILastListenedTrackRepository
{
    private readonly PostgreDbContext _dbContext;
    private DbSet<LastListenedTrack> LastListenedTracks => _dbContext.Set<LastListenedTrack>();
    private ILogger<LastListenedTrackRepository> _logger;

    public LastListenedTrackRepository(PostgreDbContext dbContext, ILogger<LastListenedTrackRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<LastListenedTrack> GetById(Guid id, CancellationToken ct)
    {
        return LastListenedTracks.FirstAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyCollection<LastListenedTrack>> GetAll(CancellationToken ct)
    {
        return await LastListenedTracks.ToListAsync(ct);
    }

    public async Task Add(LastListenedTrack entity, CancellationToken ct)
    {
        var listenedTracks = await LastListenedTracks
            .Include(e=>e.Track)
            .Where(lt => lt.User.Id == entity.User.Id)
            .OrderByDescending(lt => lt.ListenTime)
            .ToListAsync(ct);
        var existedListenedTrack = listenedTracks.Find(e => e.Track.Id == entity.Track.Id);
        if (existedListenedTrack != null)
            return;
        if (listenedTracks.Count >= 3)
        {
            var oldestListenedTrack = listenedTracks.Last();
            LastListenedTracks.Remove(oldestListenedTrack);
        }

        await LastListenedTracks.AddAsync(entity, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Update(LastListenedTrack entity, CancellationToken ct)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Delete(LastListenedTrack entity, CancellationToken ct)
    {
        LastListenedTracks.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<Track>> GetTracksByUserId(Guid userId, CancellationToken ct)
    {
        return await LastListenedTracks
            .Include(e => e.Track.User)
            .Include(e => e.Track.Album)
            .Where(e => e.User.Id == userId)
            .Select(e => e.Track)
            .ToListAsync(ct);
    }
}