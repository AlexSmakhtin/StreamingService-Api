using System.Security.Claims;
using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[Route("api")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IFileService _fileService;
    private readonly ILogger<UserController> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITrackRepository _trackRepository;
    private readonly ITrackService _trackService;
    private readonly IAlbumRepository _albumRepository;
    
    public UserController(
        IUserService userService,
        ILogger<UserController> logger,
        IFileService fileService,
        IUserRepository userRepository,
        IConfiguration configuration,
        ITransactionRepository transactionRepository,
        ITrackRepository trackRepository,
        ITrackService trackService,
        IAlbumRepository albumRepository)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(fileService);
        ArgumentNullException.ThrowIfNull(userService);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(transactionRepository);
        ArgumentNullException.ThrowIfNull(trackService);
        ArgumentNullException.ThrowIfNull(trackRepository);
        ArgumentNullException.ThrowIfNull(albumRepository);

        _configuration = configuration;
        _transactionRepository = transactionRepository;
        _trackRepository = trackRepository;
        _trackService = trackService;
        _albumRepository = albumRepository;
        _userRepository = userRepository;
        _configuration = configuration;
        _fileService = fileService;
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("user/auth")]
    public async Task<ActionResult<UserAuthResponse>> Authenticate(
        [FromBody] UserAuthRequest authRequest,
        [FromServices] IJwtService jwtService,
        CancellationToken ct)
    {
        try
        {
            var existedUser = await _userService.Authenticate(
                authRequest.Email,
                authRequest.Password,
                ct);
            var jwt = jwtService.GenerateToken(existedUser);
            return new UserAuthResponse()
            {
                JwtToken = jwt,
                UserName = existedUser.Name,
                UserId = existedUser.Id,
                Role = existedUser.Role.ToString()
            };
        }
        catch (UserNotFoundException)
        {
            return Unauthorized("User not found");
        }
        catch (IncorrectPasswordException)
        {
            return Unauthorized("Incorrect password");
        }
    }

    [HttpPost("user/register")]
    public async Task<ActionResult<UserRegisterResponse>> Register(
        [FromBody] UserRegisterRequest registerRequest,
        [FromServices] IJwtService jwtService,
        CancellationToken ct)
    {
        try
        {
            var newUser = await _userService.Register(
                registerRequest.Name,
                registerRequest.Email,
                registerRequest.Password,
                registerRequest.Status,
                registerRequest.Role,
                registerRequest.Birthday,
                ct);
            var jwt = jwtService.GenerateToken(newUser);
            var regResponse = new UserRegisterResponse()
            {
                Name = newUser.Name,
                Email = newUser.EmailAddress,
                JwtToken = jwt,
                UserName = newUser.Name,
                UserId = newUser.Id,
                Role = newUser.Role.ToString()
            };
            return regResponse;
        }
        catch (EmailAlreadyExistsException)
        {
            return BadRequest("Email already used");
        }
    }

    [Authorize]
    [HttpPost("user/change_avatar")]
    public async Task<IActionResult> ChangeAvatar(
        IFormFile file,
        CancellationToken ct)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(file);
            var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? throw new UserNotFoundException("Token does not contain the userId");
            var guid = Guid.Parse(strId);
            var user = await _userRepository.GetById(guid, ct);
            var directory = _configuration["AvatarPath"];
            if (directory == null)
                throw new ArgumentNullException(directory);
            var path = await _fileService.WriteFile(
                file,
                user.Id,
                directory,
                ct);
            await _userService.ChangeAvatar(user, path, ct);
            return Ok();
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("user/avatar")]
    public async Task<IActionResult> GetAvatar(CancellationToken ct)
    {
        try
        {
            var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? throw new UserNotFoundException("JWT token does not contain the userId");
            var guid = Guid.Parse(strId);
            var user = await _userRepository.GetById(guid, ct);
            if (!System.IO.File.Exists(user.AvatarFilePath))
            {
                return NotFound("Avatar not found");
            }

            var extension = Path.GetExtension(user.AvatarFilePath);
            var contentType = GetContentType(extension);
            var fileName = Path.GetFileName(user.AvatarFilePath);
            var fileBytes = await _fileService.ReadFile(user.AvatarFilePath, ct);
            return File(fileBytes, contentType, fileName);
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("user/{id:guid}/avatar")]
    public async Task<IActionResult> GetAvatarById(Guid id, CancellationToken ct)
    {
        try
        {
            var user = await _userRepository.GetById(id, ct);
            if (!System.IO.File.Exists(user.AvatarFilePath))
            {
                return NotFound("Avatar not found");
            }

            var extension = Path.GetExtension(user.AvatarFilePath);
            var contentType = GetContentType(extension);
            var fileName = Path.GetFileName(user.AvatarFilePath);
            var fileBytes = await _fileService.ReadFile(user.AvatarFilePath, ct);
            return File(fileBytes, contentType, fileName);
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("musicians")]
    public async Task<ActionResult<MusiciansGetResponse>> GetAllMusician(
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 6)
    {
        var musiciansCount = await _userRepository.GetTotalMusiciansCount(ct);
        var musicians = await _userRepository.GetAllMusicians(pageNumber, pageSize, ct);
        var totalPages = (int)Math.Ceiling((double)musiciansCount / pageSize);
        var response = new MusiciansGetResponse() { TotalPages = totalPages };
        foreach (var musician in musicians)
        {
            var musicianResponse = new MusicianResponse()
            {
                Id = musician.Id,
                Listening = await _userRepository.GetCountOfListeningForMusician(musician, ct),
                Name = musician.Name,
                AlbumsCount = musician.Albums.Count,
                TracksCount = musician.Tracks.Count
            };
            response.Musicians.Add(musicianResponse);
        }

        return response;
    }

    [Authorize]
    [HttpGet("musicians/{id:guid}")]
    public async Task<ActionResult<MusicianResponse>> GetMusicianById(Guid id, CancellationToken ct)
    {
        var musician = await _userRepository.GetById(id, ct);
        var response = new MusicianResponse()
        {
            Id = musician.Id,
            Listening = await _userRepository.GetCountOfListeningForMusician(musician, ct),
            Name = musician.Name,
            AlbumsCount = musician.Albums.Count,
            TracksCount = musician.Tracks.Count
        };

        return response;
    }

    [Authorize]
    [HttpGet("musicians/{id:guid}/tracks")]
    public async Task<ActionResult<TracksWithPagesCountGetResponse>> GetTracksByMusicianId(
        Guid id,
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int takeCount = 10)
    {
        var tracks = await _trackRepository.GetByUserId(id, takeCount, pageNumber, ct);
        var tracksCount = await _trackRepository.GetCountOfTracksByMusician(id, ct);
        var totalPages = (int)Math.Ceiling((double)tracksCount / takeCount);

        var response = new TracksWithPagesCountGetResponse
        {
            Tracks = [],
            TotalPages = totalPages
        };
        foreach (var track in tracks)
        {
            var fileBytes = await _fileService.ReadFile(track.FilePath, ct);
            var totalSeconds = await _trackService.GetTotalDurationOfFileInSeconds(fileBytes);
            response.Tracks.Add(new TrackGetResponseToShow()
            {
                Id = track.Id,
                AuthorName = track.User.Name,
                MusicianId = track.User.Id,
                Name = track.Name,
                TotalSeconds = totalSeconds
            });
        }

        return response;
    }

    [Authorize]
    [HttpGet("musicians/{id:guid}/albums")]
    public async Task<ActionResult<AlbumsWithPagesCountGetResponse>> GetAlbumsByMusicianId(
        Guid id,
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int takeCount = 10)
    {
        var albums = await _albumRepository.GetByUserId(id, pageNumber, takeCount, ct);
        var albumsCount = await _albumRepository.GetCountOfAlbumsByMusician(id, ct);
        var totalPages = (int)Math.Ceiling((double)albumsCount / takeCount);

        var response = new AlbumsWithPagesCountGetResponse
        {
            Albums = [],
            TotalPages = totalPages
        };
        foreach (var album in albums)
        {
            var tracks = new List<TrackGetResponseToShow>();
            foreach (var track in album.Tracks)
            {
                var fileBytes = await _fileService.ReadFile(track.FilePath, ct);
                var totalSeconds = await _trackService.GetTotalDurationOfFileInSeconds(fileBytes);
                tracks.Add(new TrackGetResponseToShow()
                {
                    AuthorName = album.User.Name,
                    Name = track.Name,
                    Id = track.Id,
                    MusicianId = track.User.Id,
                    TotalSeconds = totalSeconds
                });
            }
            response.Albums.Add(new AlbumResponse()
            {
                Id = album.Id,
                AuthorName = album.User.Name,
                CountOfTracks = tracks.Count,
                MusicianId = album.User.Id,
                Name = album.Name,
                Tracks = tracks
            });
        }

        return response;
    }
    
    [Authorize]
    [HttpGet("musicians/search")]
    public async Task<ActionResult<MusiciansGetResponse>> GetMusicianByName(
        [FromQuery] string name,
        CancellationToken ct,
        [FromQuery] int takeCount = 6)
    {
        var musicians = await _userRepository.SearchByName(takeCount, name, ct);
        var totalPages = (int)Math.Ceiling((double)musicians.Count / takeCount);
        var response = new MusiciansGetResponse() { TotalPages = totalPages };

        foreach (var musician in musicians)
        {
            var responseMusician = new MusicianResponse()
            {
                Id = musician.Id,
                Listening = await _userRepository.GetCountOfListeningForMusician(musician, ct),
                Name = musician.Name,
                AlbumsCount = musician.Albums.Count,
                TracksCount = musician.Tracks.Count
            };
            response.Musicians.Add(responseMusician);
        }

        return response;
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<ActionResult<UserGetResponse>> GetUser(CancellationToken ct)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UserNotFoundException("JWT token does not contain the userId");
        var guid = Guid.Parse(strId);
        var user = await _userRepository.GetById(guid, ct);
        return new UserGetResponse()
        {
            Email = user.EmailAddress,
            Name = user.Name
        };
    }

    [HttpPost("user/update")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(UpdateUserRequest request, CancellationToken ct)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UserNotFoundException("JWT token does not contain the userId");
        var guid = Guid.Parse(strId);
        var user = await _userRepository.GetById(guid, ct);
        user.Name = request.Name;
        user.EmailAddress = request.Email;
        await _userRepository.Update(user, ct);
        return Ok();
    }

    [HttpPost("user/change_password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UserNotFoundException("JWT token does not contain the userId");
        var guid = Guid.Parse(strId);
        var user = await _userRepository.GetById(guid, ct);
        try
        {
            await _userService.ChangePassword(user, request.OldPassword, request.NewPassword, ct);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest($"Changing password error: {ex.Message}");
        }

        return Ok();
    }

    [HttpGet("user/subscription")]
    [Authorize]
    public async Task<ActionResult<GetSubscriptionResponse>> GetSubscription(CancellationToken ct)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UserNotFoundException("JWT token does not contain the userId");
        var guid = Guid.Parse(strId);
        var user = await _userRepository.GetById(guid, ct);
        try
        {
            var transaction = await _transactionRepository.GetActiveSubscription(user.Id, ct);
            return new GetSubscriptionResponse()
            {
                ExpireDate = transaction.ExpirationDate,
                Name = transaction.Subscription.Name
            };
        }
        catch (InvalidOperationException)
        {
            return NotFound($"No active subscription for user: {user.Id}");
        }
    }

    private string GetContentType(string extension)
    {
        switch (extension.ToLower())
        {
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".png":
                return "image/png";
            default:
                return "application/octet-stream";
        }
    }
}