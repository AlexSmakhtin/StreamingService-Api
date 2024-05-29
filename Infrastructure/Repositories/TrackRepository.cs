using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TrackRepository : ITrackRepository
{
    private readonly PostgreDbContext _dbContext;

    private DbSet<Track> Tracks => _dbContext.Set<Track>();

    public TrackRepository(PostgreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Track> GetById(Guid id, CancellationToken ct)
    {
        return Tracks.FirstAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyCollection<Track>> GetAll(CancellationToken ct)
    {
        return await Tracks.ToListAsync(ct);
    }

    public async Task Add(Track entity, CancellationToken ct)
    {
        await Tracks.AddAsync(entity, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Update(Track entity, CancellationToken ct)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Delete(Track entity, CancellationToken ct)
    {
        Tracks.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<Track>> GetPopularByMusicianId(Guid musicianId, int takeCount, CancellationToken ct)
    {
        return await Tracks
            .Include(e => e.User)
            .Where(e => e.User.Id == musicianId)
            .OrderByDescending(e => e.CountOfListen)
            .Take(takeCount)
            .ToListAsync(ct);
    }

    public async Task<List<Track>> GetByUserId(Guid musicianId, int takeCount, int pageNumber, CancellationToken ct)
    {
        return await Tracks
            .Include(e => e.User)
            .OrderBy(e => e.Name)
            .Where(e => e.User.Id == musicianId)
            .Skip((pageNumber - 1) * takeCount)
            .Take(takeCount)
            .ToListAsync(ct);
    }

    public async Task<int> GetCountOfTracksByMusician(Guid musicianId, CancellationToken ct)
    {
        return await Tracks
            .Include(e => e.User)
            .Where(e => e.User.Id == musicianId)
            .CountAsync(ct);
    }

    public async Task<List<Track>> SearchByName(int takeCount, string name, CancellationToken ct)
    {
        return await Tracks
            .Include(e => e.User)
            .OrderBy(e => e.Name)
            .Where(e => EF.Functions.Like(e.Name.ToLower(), $"{name.ToLower()}%"))
            .Take(takeCount)
            .ToListAsync(ct);
    }

    public async Task<List<Track>> GetPopularForAll(int takeCount, CancellationToken ct)
    {
        return await Tracks
            .Include(e => e.User)
            .OrderByDescending(e => e.CountOfListen)
            .Take(takeCount)
            .ToListAsync(ct);
    }
}