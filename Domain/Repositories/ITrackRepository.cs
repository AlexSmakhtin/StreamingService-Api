using Domain.Entities;

namespace Domain.Repositories;

public interface ITrackRepository : IRepository<Track>
{
    Task<List<Track>> GetPopularByMusicianId(Guid musicianId, int takeCount, CancellationToken ct);
    Task<List<Track>> GetByUserId(Guid musicianId, int takeCount, int pageNumber, CancellationToken ct);

    Task<int> GetCountOfTracksByMusician(Guid musicianId, CancellationToken ct);
    Task<List<Track>> SearchByName(int takeCount, string name, CancellationToken ct);
    Task<List<Track>> GetPopularForAll(int takeCount, CancellationToken ct);
}