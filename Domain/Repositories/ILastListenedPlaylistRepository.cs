using Domain.Entities;

namespace Domain.Repositories;

public interface ILastListenedPlaylistRepository:IRepository<LastListenedPlaylist>
{
    Task<List<Playlist>> GetPlaylistsByUserId(Guid userId, CancellationToken ct);
}