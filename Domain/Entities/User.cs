using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Enums;

namespace Domain.Entities;

public class User : IEntity
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

    private static readonly EmailAddressAttribute EmailAddressAttribute = new();

    private string _emailAddress;

    public string EmailAddress
    {
        get => _emailAddress;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty", nameof(value));
            if (!EmailAddressAttribute.IsValid(value))
                throw new ArgumentException("Email is not valid", nameof(value));
            _emailAddress = value;
        }
    }

    private string _avatarFilePath;

    public string AvatarFilePath
    {
        get => _avatarFilePath;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _avatarFilePath = value;
        }
    }

    private Roles _role;

    public Roles Role
    {
        get => _role;
        set => _role = value;
    }

    private Statuses _status;
    public Statuses Status
    {
        get => _status;
        set => _status = value;
    }

    private DateTime _birthday;

    public DateTime Birthday
    {
        get => _birthday;
        set
        {
            if (value == default)
                throw new ArgumentException("Birthday cannot be default value", nameof(value));
            if (value > DateTime.Now.AddYears(-18))
                throw new ArgumentException("User must be at least 18 years old", nameof(value));
            _birthday = value;
        }
    }

    private string _hashedPassword;

    public string HashedPassword
    {
        get => _hashedPassword;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Password cannot be empty", nameof(value));
            _hashedPassword = value;
        }
    }

    private int _freeTracks;

    public int FreeTracks
    {
        get => _freeTracks;
        set => _freeTracks = value;
    }


    public List<Album> Albums { get; set; } = [];
    public List<Track> Tracks { get; set; } = [];
    public List<LastListenedTrack> LastListenedTracksList { get; set; } = [];
    public List<LastListenedPlaylist> LastListenedPlaylists { get; set; } = [];
    public List<LastListenedAlbum> LastListenedAlbums { get; set; } = [];
    public Subscription? Subscription { get; set; }
    public List<Transaction> Transactions { get; set; } = [];

    public User(
        string name,
        string emailAddress,
        string hashedPassword,
        Statuses status,
        Roles role,
        DateTime birthday,
        string avatarFilePath = "",
        int freeTracks = 15)
    {
        FreeTracks = freeTracks;
        Name = name;
        EmailAddress = emailAddress;
        HashedPassword = hashedPassword;
        Status = status;
        Role = role;
        AvatarFilePath = avatarFilePath;
        Birthday = birthday;
    }
}