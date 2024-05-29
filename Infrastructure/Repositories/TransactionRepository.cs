using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly PostgreDbContext _dbContext;

    private DbSet<Transaction> Transactions => _dbContext.Set<Transaction>();

    public TransactionRepository(PostgreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Transaction> GetById(Guid id, CancellationToken ct)
    {
        return Transactions.FirstAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyCollection<Transaction>> GetAll(CancellationToken ct)
    {
        return await Transactions.ToListAsync(ct);
    }

    public async Task Add(Transaction entity, CancellationToken ct)
    {
        await Transactions.AddAsync(entity, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Update(Transaction entity, CancellationToken ct)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Delete(Transaction entity, CancellationToken ct)
    {
        Transactions.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<Transaction> GetActiveSubscription(Guid userId, CancellationToken ct)
    {
        var transaction = await Transactions
            .Include(e => e.User)
            .Include(e => e.Subscription)
            .Where(e => e.User.Id == userId)
            .OrderByDescending(e => e.PurchaseDate)
            .FirstAsync(ct);
        if (transaction.ExpirationDate <= DateTime.Now)
            throw new InvalidOperationException("Subscribe expired");
        return transaction;
    }
}