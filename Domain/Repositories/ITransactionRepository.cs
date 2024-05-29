using Domain.Entities;

namespace Domain.Repositories;

public interface ITransactionRepository:IRepository<Transaction>
{
    Task<Transaction> GetActiveSubscription(Guid userId, CancellationToken ct);
}