using System.Security.Claims;
using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[Route("api/playlists")]
[ApiController]
public class PlaylistController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<TrackController> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ITrackRepository _trackRepository;
    private readonly ITrackService _trackService;
    private readonly ILastListenedTrackRepository _lastListenedTrackRepository;
    private readonly ILastListenedPlaylistRepository _lastListenedPlaylistRepository;
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IPlaylistTrackRepository _playlistTrackRepository;

    public PlaylistController(
        IFileService fileService,
        ILogger<TrackController> logger,
        IUserRepository userRepository,
        IConfiguration configuration,
        ITrackRepository trackRepository,
        ITrackService trackService,
        ILastListenedTrackRepository lastListenedTrackRepository,
        ILastListenedPlaylistRepository lastListenedPlaylistRepository,
        IPlaylistRepository playlistRepository,
        IPlaylistTrackRepository playlistTrackRepository)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
        _trackService = trackService ?? throw new ArgumentNullException(nameof(trackService));
        _lastListenedTrackRepository = lastListenedTrackRepository ??
                                       throw new ArgumentNullException(nameof(lastListenedTrackRepository));
        _lastListenedPlaylistRepository = lastListenedPlaylistRepository ??
                                          throw new ArgumentNullException(nameof(lastListenedPlaylistRepository));
        _playlistRepository = playlistRepository ??
                              throw new ArgumentNullException(nameof(playlistRepository));
        _playlistTrackRepository = playlistTrackRepository ??
                                   throw new ArgumentNullException(nameof(playlistTrackRepository));
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PlaylistsWithCountOfPagesResponse>> GetPlaylistsForUser(
        CancellationToken ct,
        [FromQuery] int takeCount = 8,
        [FromQuery] int pageNumber = 1)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UserNotFoundException("Token does not contain the userId");
        var userId = Guid.Parse(strId);
        var playlists = await _playlistRepository.GetByUserId(takeCount, pageNumber, userId, ct);
        var count = await _playlistRepository.GetCountOfUserPlaylists(userId, ct);
        var totalPages = (int)Math.Ceiling((double)count / takeCount);

        var response = new PlaylistsWithCountOfPagesResponse()
        {
            Playlists = [],
            TotalPages = totalPages
        };
        foreach (var playlist in playlists)
        {
            var trackList = new List<TrackGetResponseToShow>();
            foreach (var track in playlist.PlaylistTracks.Select(e => e.Track))
            {
                var fileBytes = await _fileService.ReadFile(track.FilePath, ct);
                var totalSeconds = await _trackService.GetTotalDurationOfFileInSeconds(fileBytes);
                trackList.Add(new TrackGetResponseToShow()
                {
                    MusicianId = playlist.User.Id,
                    AuthorName = playlist.User.Name,
                    Id = track.Id,
                    Name = track.Name,
                    TotalSeconds = totalSeconds
                });
            }

            response.Playlists.Add(new PlaylistResponse()
            {
                CountOfTracks = playlist.PlaylistTracks.Count,
                Id = playlist.Id,
                Name = playlist.Name,
                Tracks = trackList.OrderBy(e => e.Name).ToList()
            });
        }

        return response;
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreatePlaylist(CancellationToken ct)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UserNotFoundException("Token does not contain the userId");
        var userId = Guid.Parse(strId);
        var user = await _userRepository.GetById(userId, ct);
        var playlist = new Playlist("New Playlist")
        {
            User = user
        };
        await _playlistRepository.Add(playlist, ct);
        return Ok();
    }

    [HttpGet("last_listened")]
    [Authorize]
    public async Task<ActionResult<List<PlaylistResponse>>> GetLastListenedPlaylists(CancellationToken ct)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UserNotFoundException("Token does not contain the userId");
        var userId = Guid.Parse(strId);
        var playlists = await _lastListenedPlaylistRepository.GetPlaylistsByUserId(userId, ct);
        var response = new List<PlaylistResponse>();
        foreach (var playlist in playlists)
        {
            var trackList = new List<TrackGetResponseToShow>();
            foreach (var track in playlist.PlaylistTracks.Select(e => e.Track))
            {
                var fileBytes = await _fileService.ReadFile(track.FilePath, ct);
                var totalSeconds = await _trackService.GetTotalDurationOfFileInSeconds(fileBytes);
                trackList.Add(new TrackGetResponseToShow()
                {
                    MusicianId = playlist.User.Id,
                    AuthorName = playlist.User.Name,
                    Id = track.Id,
                    Name = track.Name,
                    TotalSeconds = totalSeconds
                });
            }

            response.Add(new PlaylistResponse()
            {
                Id = playlist.Id,
                Name = playlist.Name,
                Tracks = trackList,
                CountOfTracks = trackList.Count
            });
        }

        return response;
    }

    [HttpPost("set_last_listened")]
    [Authorize]
    public async Task<IActionResult> SetLastListenedPlaylist(
        [FromQuery] Guid playlistId,
        CancellationToken ct)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UserNotFoundException("Token does not contain the userId");
        var userId = Guid.Parse(strId);
        var user = await _userRepository.GetById(userId, ct);
        var playlist = await _playlistRepository.GetById(playlistId, ct);
        var lastListened = new LastListenedPlaylist(DateTime.Now.ToUniversalTime())
        {
            User = user,
            Playlist = playlist
        };
        await _lastListenedPlaylistRepository.Add(lastListened, ct);
        return Ok();
    }

    [HttpPost("update")]
    [Authorize]
    public async Task<IActionResult> UpdatePlaylist(
        [FromBody] PlaylistUpdateRequest request,
        [FromQuery] Guid playlistId,
        CancellationToken ct)
    {
        var playlist = await _playlistRepository.GetById(playlistId, ct);
        playlist.Name = request.Name;

        var tracksToRemove = new List<PlaylistTrack>();

        foreach (var trackIdToRemove in request.TrackIds)
        {
            var playlistTrack =
                await _playlistTrackRepository.GetByTrackIdAndPlaylistId(playlist.Id, trackIdToRemove, ct);
            tracksToRemove.Add(playlistTrack);
        }

        foreach (var playlistTrack in tracksToRemove)
        {
            playlist.PlaylistTracks.Remove(playlistTrack);
            await _playlistTrackRepository.Delete(playlistTrack, ct);
        }

        await _playlistRepository.Update(playlist, ct);

        return Ok();
    }

    [HttpPost("delete")]
    [Authorize]
    public async Task<IActionResult> DeletePlaylist([FromQuery] Guid playlistId, CancellationToken ct)
    {
        var playlist = await _playlistRepository.GetById(playlistId, ct);
        foreach (var playlistTrack in playlist.PlaylistTracks)
        {
            await _playlistTrackRepository.Delete(playlistTrack, ct);
        }

        await _playlistRepository.Delete(playlist, ct);
        return Ok();
    }

    [HttpPost("add_track")]
    [Authorize]
    public async Task<IActionResult> AddTrackToPlaylist(
        [FromQuery] Guid playlistId,
        [FromQuery] Guid trackId,
        CancellationToken ct)
    {
        var playlist = await _playlistRepository.GetById(playlistId, ct);
        var track = await _trackRepository.GetById(trackId, ct);
        await _playlistTrackRepository.Add(new PlaylistTrack()
        {
            Playlist = playlist,
            Track = track
        }, ct);
        return Ok();
    }
}