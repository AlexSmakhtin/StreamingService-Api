using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly PostgreDbContext _dbContext;

    private DbSet<Subscription> Subscriptions => _dbContext.Set<Subscription>();

    public SubscriptionRepository(PostgreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Subscription> GetById(Guid id, CancellationToken ct)
    {
        return Subscriptions.FirstAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyCollection<Subscription>> GetAll(CancellationToken ct)
    {
        return await Subscriptions.ToListAsync(ct);
    }

    public async Task Add(Subscription entity, CancellationToken ct)
    {
        await Subscriptions.AddAsync(entity, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Update(Subscription entity, CancellationToken ct)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Delete(Subscription entity, CancellationToken ct)
    {
        Subscriptions.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
    }
}