namespace Domain.Entities;

public class Track : IEntity
{
    public Guid Id { get; init; }
    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _name = value;
        }
    }

    private string _filePath;

    public string FilePath
    {
        get => _filePath;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _filePath = value;
        }
    }

    private long _countOfListen;

    public long CountOfListen
    {
        get => _countOfListen;
        set => _countOfListen = value;
    }

    public User User { get; set; } = null!;
    public Album? Album { get; set; }
    public List<PlaylistTrack> PlaylistTracks { get; } = [];

    public Track(string name, string filePath)
    {
        CountOfListen = 0;
        Name = name;
        FilePath = filePath;
    }
}