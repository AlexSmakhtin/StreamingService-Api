using Domain.Entities;

namespace Domain.Repositories;

public interface ILastListenedAlbumRepository : IRepository<LastListenedAlbum>
{
    Task<List<Album>> GetAlbumsByUserId(Guid userId, CancellationToken ct);
}