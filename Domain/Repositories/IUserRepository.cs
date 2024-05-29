using Domain.Entities;

namespace Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> FindByEmail(string email, CancellationToken ct);

    Task<List<User>> GetAllMusicians(int pageNumber, int pageSize, CancellationToken ct);
    public Task<long> GetCountOfListeningForMusician(User user, CancellationToken ct);

    public Task<int> GetTotalMusiciansCount(CancellationToken ct);

    public Task<List<User>> SearchByName(int takeCount, string name, CancellationToken ct);

    public Task AddFreeTracksForAll(CancellationToken ct);
}