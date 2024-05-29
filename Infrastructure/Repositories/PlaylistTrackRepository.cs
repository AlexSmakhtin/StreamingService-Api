using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PlaylistTrackRepository : IPlaylistTrackRepository
{
    private readonly PostgreDbContext _dbContext;

    private DbSet<PlaylistTrack> PlaylistTracks => _dbContext.Set<PlaylistTrack>();

    public PlaylistTrackRepository(PostgreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PlaylistTrack> GetById(Guid id, CancellationToken ct)
    {
        return PlaylistTracks
            .Include(e => e.Track)
            .Include(e => e.Playlist)
            .FirstAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyCollection<PlaylistTrack>> GetAll(CancellationToken ct)
    {
        return await PlaylistTracks.ToListAsync(ct);
    }

    public async Task Add(PlaylistTrack entity, CancellationToken ct)
    {
        await PlaylistTracks.AddAsync(entity, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Update(PlaylistTrack entity, CancellationToken ct)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Delete(PlaylistTrack entity, CancellationToken ct)
    {
        PlaylistTracks.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<PlaylistTrack> GetByTrackIdAndPlaylistId(Guid playlistId, Guid trackId, CancellationToken ct)
    {
        return await PlaylistTracks
            .Include(e => e.Playlist)
            .Include(e => e.Track)
            .Where(e => e.Track.Id == trackId && e.Playlist.Id == playlistId)
            .FirstAsync(ct);
    }
}