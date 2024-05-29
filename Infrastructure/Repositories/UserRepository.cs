using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly PostgreDbContext _dbContext;

    private DbSet<User> Users => _dbContext.Set<User>();

    public UserRepository(PostgreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User> GetById(Guid id, CancellationToken ct)
    {
        return Users
            .Include(e => e.Albums)
            .Include(e => e.Tracks)
            .FirstAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyCollection<User>> GetAll(CancellationToken ct)
    {
        return await Users.ToListAsync(ct);
    }

    public async Task Add(User entity, CancellationToken ct)
    {
        await Users.AddAsync(entity, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Update(User entity, CancellationToken ct)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Delete(User entity, CancellationToken ct)
    {
        Users.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
    }

    public Task<User?> FindByEmail(string email, CancellationToken ct)
    {
        return Users.FirstOrDefaultAsync(e => e.EmailAddress == email, ct);
    }

    public async Task<List<User>> GetAllMusicians(int pageNumber, int pageSize, CancellationToken ct)
    {
        return await Users
            .Include(e => e.Tracks)
            .Include(e => e.Albums)
            .OrderBy(e => e.Name)
            .Where(e => e.Role == Roles.Musician)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<long> GetCountOfListeningForMusician(User user, CancellationToken ct)
    {
        if (user.Role != Roles.Musician)
            throw new ArgumentException("User must be a musician", nameof(user));
        var existedUser = await Users.FirstAsync(e => e.Id == user.Id, ct);
        var tracks = existedUser.Tracks;
        var count = tracks.Sum(track => track.CountOfListen);
        return count;
    }

    public async Task<int> GetTotalMusiciansCount(CancellationToken ct)
    {
        return await Users
            .Where(e => e.Role == Roles.Musician)
            .CountAsync(ct);
    }

    public Task<List<User>> SearchByName(int takeCount, string name, CancellationToken ct)
    {
        return Users
            .Include(e => e.Tracks)
            .Include(e => e.Albums)
            .OrderBy(e => e.Name)
            .Where(e => e.Role == Roles.Musician)
            .Where(e => EF.Functions.Like(e.Name.ToLower(), $"{name.ToLower()}%"))
            .Take(takeCount)
            .ToListAsync(ct);
    }

    public async Task AddFreeTracksForAll(CancellationToken ct)
    {
        await Users.ForEachAsync(e => e.FreeTracks = 15, ct);
        await _dbContext.SaveChangesAsync(ct);
    }
}