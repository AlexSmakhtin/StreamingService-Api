using System.Security.Claims;
using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[Route("api/albums")]
[ApiController]
public class AlbumController : ControllerBase
{
    private readonly IAlbumRepository _albumRepository;
    private readonly IConfiguration _configuration;
    private readonly ITrackService _trackService;
    private readonly IFileService _fileService;
    private readonly ILogger<AlbumController> _logger;
    private readonly ILastListenedAlbumRepository _lastListenedAlbumRepository;
    private readonly IUserRepository _userRepository;


    public AlbumController(
        IAlbumRepository albumRepository,
        IConfiguration configuration,
        ITrackService trackService,
        IFileService fileService,
        ILogger<AlbumController> logger,
        ILastListenedAlbumRepository lastListenedAlbumRepository, IUserRepository userRepository)
    {
        ArgumentNullException.ThrowIfNull(albumRepository);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(trackService);
        ArgumentNullException.ThrowIfNull(fileService);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(lastListenedAlbumRepository);
        ArgumentNullException.ThrowIfNull(userRepository);

        _albumRepository = albumRepository;
        _configuration = configuration;
        _trackService = trackService;
        _fileService = fileService;
        _logger = logger;
        _lastListenedAlbumRepository = lastListenedAlbumRepository;
        _userRepository = userRepository;
    }

    [Authorize]
    [HttpGet("popular")]
    public async Task<ActionResult<List<AlbumResponse>>> GetPopularAlbumsByMusician(
        [FromQuery] Guid musicianId,
        CancellationToken ct,
        [FromQuery] int takeCount = 3)
    {
        var albums = await _albumRepository.GetPopularByMusicianId(musicianId, takeCount, ct);
        var response = new List<AlbumResponse>();
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

            response.Add(new AlbumResponse()
            {
                MusicianId = album.User.Id,
                Name = album.Name,
                AuthorName = album.User.Name,
                Id = album.Id,
                Tracks = tracks.OrderBy(e => e.Name).ToList(),
                CountOfTracks = tracks.Count
            });
            _logger.LogInformation("The magic number is {@album}", album.Name);
        }

        return response;
    }

    [HttpGet("last_listened")]
    [Authorize]
    public async Task<ActionResult<List<AlbumResponse>>> GetLastListenedAlbums(CancellationToken ct)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UserNotFoundException("Token does not contain the userId");
        var userId = Guid.Parse(strId);
        var albums = await _lastListenedAlbumRepository.GetAlbumsByUserId(userId, ct);
        var response = new List<AlbumResponse>();
        foreach (var album in albums)
        {
            var trackList = new List<TrackGetResponseToShow>();
            foreach (var track in album.Tracks)
            {
                var fileBytes = await _fileService.ReadFile(track.FilePath, ct);
                var totalSeconds = await _trackService.GetTotalDurationOfFileInSeconds(fileBytes);
                trackList.Add(new TrackGetResponseToShow()
                {
                    MusicianId = track.User.Id,
                    AuthorName = track.User.Name,
                    Id = track.Id,
                    Name = track.Name,
                    TotalSeconds = totalSeconds
                });
            }

            response.Add(new AlbumResponse()
            {
                MusicianId = album.User.Id,
                Id = album.Id,
                Name = album.Name,
                AuthorName = album.User.Name,
                Tracks = trackList.OrderBy(e => e.Name).ToList(),
                CountOfTracks = trackList.Count
            });
        }

        return response;
    }

    [HttpPost("set_last_listened")]
    [Authorize]
    public async Task<IActionResult> SetLastListenedAlbum(
        [FromQuery] Guid albumId,
        CancellationToken ct)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UserNotFoundException("Token does not contain the userId");
        var userId = Guid.Parse(strId);
        var user = await _userRepository.GetById(userId, ct);
        var album = await _albumRepository.GetById(albumId, ct);
        var lastListened = new LastListenedAlbum(DateTime.Now.ToUniversalTime())
        {
            User = user,
            Album = album
        };
        await _lastListenedAlbumRepository.Add(lastListened, ct);
        return Ok();
    }
}