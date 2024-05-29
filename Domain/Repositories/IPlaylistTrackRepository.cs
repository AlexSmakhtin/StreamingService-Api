using Domain.Entities;

namespace Domain.Repositories;

public interface IPlaylistTrackRepository : IRepository<PlaylistTrack>
{
    Task<PlaylistTrack> GetByTrackIdAndPlaylistId(Guid playlistId, Guid trackId, CancellationToken ct);
}