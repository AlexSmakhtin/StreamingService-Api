using Domain.Entities;

namespace Domain.Repositories;

public interface IAlbumRepository : IRepository<Album>
{
    public Task<List<Album>> GetPopularByMusicianId(Guid userId, int takeCount, CancellationToken ct);

    public Task<List<Album>> GetByUserId(Guid musicianId, int pageNumber, int takeCount, CancellationToken ct);

    public Task<int> GetCountOfAlbumsByMusician(Guid musicianId, CancellationToken ct);
}