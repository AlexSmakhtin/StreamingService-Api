namespace Domain.Entities;

public class Album : IEntity
{
    public Guid Id { get; init; }

    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be empty", nameof(value));
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

    public User User { get; set; } = null!;
    public List<Track> Tracks { get; set; } = [];

    public Album(string name, string imagePath = "")
    {
        ImagePath = imagePath;
        Name = name;
    }
}