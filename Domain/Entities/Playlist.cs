namespace Domain.Entities;

public class Playlist : IEntity
{
    public Guid Id { get; init; }

    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _name = value;
        }
    }

    private string _imagePath;

    public string ImagePath
    {
        get => _imagePath;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _imagePath = value;
        }
    }

    public List<PlaylistTrack> PlaylistTracks { get; set; } = [];
    public User User { get; set; } = null!;

    public Playlist(string name, string imagePath = "")
    {
        ImagePath = imagePath;
        Name = name;
    }
}