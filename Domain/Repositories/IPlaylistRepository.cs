using Domain.Entities;

namespace Domain.Repositories;

public interface IPlaylistRepository : IRepository<Playlist>
{
    Task<List<Playlist>> GetByUserId(int takeCount, int pageNumber, Guid userId, CancellationToken ct);
    Task<int> GetCountOfUserPlaylists(Guid userId, CancellationToken ct);
}