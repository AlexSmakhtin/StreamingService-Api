namespace Domain.Entities;

public class PlaylistTrack : IEntity
{
    public Guid Id { get; init; }

    public Playlist Playlist { get; init; } = null!;

    public Track Track { get; init; } = null!;
}