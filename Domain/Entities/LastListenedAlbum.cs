namespace Domain.Entities;

public class LastListenedAlbum : IEntity
{
    public Guid Id { get; init; }
    private DateTime _listenTime;

    public DateTime ListenTime
    {
        get => _listenTime;
        set
        {
            if (value == default)
                throw new ArgumentException("Listen Time cannot be default value", nameof(value));
            if (value > DateTime.Now)
                throw new ArgumentException("Listen Time cannot be greater then now", nameof(value));
            _listenTime = value;
        }
    }

    public User User { get; set; } = null!;
    public Album Album { get; set; } = null!;

    public LastListenedAlbum(DateTime listenTime)
    {
        ListenTime = listenTime;
    }
}