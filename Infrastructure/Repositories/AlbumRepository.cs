using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AlbumRepository : IAlbumRepository
{
    private readonly PostgreDbContext _dbContext;

    private DbSet<Album> Albums => _dbContext.Set<Album>();

    public AlbumRepository(PostgreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Album> GetById(Guid id, CancellationToken ct)
    {
        return Albums.FirstAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyCollection<Album>> GetAll(CancellationToken ct)
    {
        return await Albums.ToListAsync(ct);
    }

    public async Task Add(Album entity, CancellationToken ct)
    {
        await Albums.AddAsync(entity, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Update(Album entity, CancellationToken ct)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Delete(Album entity, CancellationToken ct)
    {
        Albums.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<Album>> GetPopularByMusicianId(Guid userId, int takeCount, CancellationToken ct)
    {
        return await Albums
            .Include(e => e.Tracks)
            .Include(e => e.User)
            .Where(e => e.User.Id == userId)
            .OrderByDescending(e => e.Tracks.Sum(t => t.CountOfListen))
            .Take(takeCount)
            .ToListAsync(ct);
    }

    public async Task<List<Album>> GetByUserId(Guid musicianId, int pageNumber, int takeCount, CancellationToken ct)
    {
        return await Albums
            .Include(e => e.User)
            .Include(e => e.Tracks)
            .OrderBy(e => e.Name)
            .Where(e => e.User.Id == musicianId)
            .Skip((pageNumber - 1) * takeCount)
            .Take(takeCount)
            .ToListAsync(ct);
    }

    public async Task<int> GetCountOfAlbumsByMusician(Guid musicianId, CancellationToken ct)
    {
        return await Albums
            .Include(e => e.User)
            .Where(e => e.User.Id == musicianId)
            .CountAsync(ct);
    }
}