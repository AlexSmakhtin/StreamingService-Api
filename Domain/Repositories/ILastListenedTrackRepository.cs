using Domain.Entities;

namespace Domain.Repositories;

public interface ILastListenedTrackRepository:IRepository<LastListenedTrack>
{
    Task<List<Track>> GetTracksByUserId(Guid userId, CancellationToken ct);
}