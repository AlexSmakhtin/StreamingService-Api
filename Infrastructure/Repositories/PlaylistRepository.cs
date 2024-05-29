using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PlaylistRepository : IPlaylistRepository
{
    private readonly PostgreDbContext _dbContext;

    private DbSet<Playlist> Playlists => _dbContext.Set<Playlist>();

    public PlaylistRepository(PostgreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Playlist> GetById(Guid id, CancellationToken ct)
    {
        return Playlists
            .Include(e => e.PlaylistTracks)
            .ThenInclude(e => e.Track)
            .FirstAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyCollection<Playlist>> GetAll(CancellationToken ct)
    {
        return await Playlists.ToListAsync(ct);
    }

    public async Task Add(Playlist entity, CancellationToken ct)
    {
        await Playlists.AddAsync(entity, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Update(Playlist entity, CancellationToken ct)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Delete(Playlist entity, CancellationToken ct)
    {
        Playlists.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<Playlist>> GetByUserId(int takeCount, int pageNumber, Guid userId, CancellationToken ct)
    {
        return await Playlists
            .Include(e => e.User)
            .Include(e => e.PlaylistTracks)
            .ThenInclude(e => e.Track)
            .Where(e => e.User.Id == userId)
            .OrderBy(e => e.Name)
            .Skip((pageNumber - 1) * takeCount)
            .Take(takeCount)
            .ToListAsync(ct);
    }

    public async Task<int> GetCountOfUserPlaylists(Guid userId, CancellationToken ct)
    {
        return await Playlists
            .Include(e => e.User)
            .Where(e => e.User.Id == userId)
            .CountAsync(ct);
    }
}