using System.Security.Claims;
using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[Route("api/tracks")]
[ApiController]
public class TrackController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<TrackController> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ITrackRepository _trackRepository;
    private readonly ITrackService _trackService;
    private readonly ILastListenedTrackRepository _lastListenedTrackRepository;
    private readonly ITransactionRepository _transactionRepository;

    public TrackController(
        IFileService fileService,
        ILogger<TrackController> logger,
        ITrackRepository trackRepository,
        IUserRepository userRepository,
        ITrackService trackService,
        IConfiguration configuration,
        ILastListenedTrackRepository lastListenedTrackRepository,
        ILastListenedPlaylistRepository lastListenedPlaylistRepository,
        ILastListenedAlbumRepository lastListenedAlbumRepository,
        ITransactionRepository transactionRepository)
    {
        ArgumentNullException.ThrowIfNull(lastListenedPlaylistRepository);
        ArgumentNullException.ThrowIfNull(lastListenedAlbumRepository);
        ArgumentNullException.ThrowIfNull(lastListenedTrackRepository);
        ArgumentNullException.ThrowIfNull(trackService);
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(fileService);
        ArgumentNullException.ThrowIfNull(trackRepository);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(transactionRepository);

        _transactionRepository = transactionRepository;
        _lastListenedTrackRepository = lastListenedTrackRepository;
        _fileService = fileService;
        _logger = logger;
        _trackRepository = trackRepository;
        _configuration = configuration;
        _userRepository = userRepository;
        _trackService = trackService;
    }

    [HttpPost("add_track")]
    [Authorize(Roles = nameof(Roles.Musician))]
    public async Task<IActionResult> AddTrack(
        [FromBody] TrackAddRequest request,
        CancellationToken ct)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(request);
            var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? throw new UserNotFoundException("Token does not contain the userId");
            var userGuid = Guid.Parse(strId);
            var user = await _userRepository.GetById(userGuid, ct);
            var directory = _configuration["AvatarPath"];
            if (directory == null)
                throw new ArgumentNullException(directory);
            var path = await _fileService.WriteFile(
                request.TrackFile,
                user.Id,
                directory,
                ct);
            var track = new Track(request.TrackName, path)
            {
                User = user
            };
            await _trackRepository.Add(track, ct);
            return Ok("Track added");
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("get_by_id_to_show")]
    [Authorize]
    public async Task<ActionResult<TrackGetResponseToShow>> GetByIdToShow(
        [FromQuery] Guid id,
        CancellationToken ct)
    {
        var track = await _trackRepository.GetById(id, ct);
        if (!System.IO.File.Exists(track.FilePath))
        {
            return NotFound("Track not found");
        }

        var fileBytes = await _fileService.ReadFile(track.FilePath, ct);
        var totalSeconds = await _trackService.GetTotalDurationOfFileInSeconds(fileBytes);
        var response = new TrackGetResponseToShow
        {
            Id = track.Id,
            MusicianId = track.User.Id,
            AuthorName = track.User.Name,
            Name = track.Name,
            TotalSeconds = totalSeconds
        };
        return response;
    }

    [HttpGet("popular_for_all")]
    [Authorize]
    public async Task<ActionResult<List<TrackGetResponseToShow>>> GetPopularForAll(
        CancellationToken ct,
        [FromQuery] int takeCount = 8)
    {
        var tracks = await _trackRepository.GetPopularForAll(takeCount, ct);
        var response = new List<TrackGetResponseToShow>();
        foreach (var track in tracks)
        {
            var fileBytes = await _fileService.ReadFile(track.FilePath, ct);
            var totalSeconds = await _trackService.GetTotalDurationOfFileInSeconds(fileBytes);
            response.Add(new TrackGetResponseToShow()
            {
                AuthorName = track.User.Name,
                Name = track.Name,
                Id = track.Id,
                MusicianId = track.User.Id,
                TotalSeconds = totalSeconds
            });
        }

        return response;
    }


    [HttpGet("search")]
    [Authorize]
    public async Task<ActionResult<List<TrackGetResponseToShow>>> SearchTracks(
        [FromQuery] string name,
        CancellationToken ct,
        [FromQuery] int takeCount = 8)
    {
        var tracks = await _trackRepository.SearchByName(takeCount, name, ct);
        var response = new List<TrackGetResponseToShow>();
        foreach (var track in tracks)
        {
            var fileBytes = await _fileService.ReadFile(track.FilePath, ct);
            var totalSeconds = await _trackService.GetTotalDurationOfFileInSeconds(fileBytes);
            response.Add(new TrackGetResponseToShow()
            {
                AuthorName = track.User.Name,
                Name = track.Name,
                Id = track.Id,
                MusicianId = track.User.Id,
                TotalSeconds = totalSeconds
            });
        }

        return response;
    }

    [HttpGet("get_by_id_to_listen")]
    public async Task<IActionResult> GetByIdToListen(
        [FromQuery] Guid id,
        [FromQuery] long position,
        [FromQuery] Guid userId,
        CancellationToken ct)
    {
        var track = await _trackRepository.GetById(id, ct);
        track.CountOfListen++;
        await _trackRepository.Update(track, ct);
        var user = await _userRepository.GetById(userId, ct);
        try
        {
            await _transactionRepository.GetActiveSubscription(user.Id, ct);
        }
        catch (InvalidOperationException)
        {
            if (user.FreeTracks != 0)
            {
                user.FreeTracks--;
                await _userRepository.Update(user, ct);
            }
            else
                return Forbid("Free tracks is 0, and no active subscription");
        }

        if (!System.IO.File.Exists(track.FilePath))
        {
            return NotFound("Track not found");
        }

        var fileBytes = await _fileService.ReadFile(track.FilePath, ct);
        var ms = new MemoryStream(fileBytes);
        var listenedTrack = new LastListenedTrack(DateTime.Now.ToUniversalTime())
        {
            Track = track,
            User = user
        };
        await _lastListenedTrackRepository.Add(listenedTrack, ct);
        ms.Seek(position, SeekOrigin.Begin);
        HttpContext.Response.Headers.AcceptRanges = "bytes";
        return File(ms, "audio/mpeg");
    }

    [HttpGet("last_listened")]
    [Authorize]
    public async Task<ActionResult<List<TrackGetResponseToShow>>> GetLastListenedTracks(CancellationToken ct)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UserNotFoundException("Token does not contain the userId");
        var userId = Guid.Parse(strId);
        var tracks = await _lastListenedTrackRepository.GetTracksByUserId(userId, ct);
        var response = new List<TrackGetResponseToShow>();
        foreach (var track in tracks)
        {
            var fileBytes = await _fileService.ReadFile(track.FilePath, ct);
            var totalSeconds = await _trackService.GetTotalDurationOfFileInSeconds(fileBytes);
            response.Add(new TrackGetResponseToShow()
            {
                MusicianId = track.User.Id,
                AuthorName = track.User.Name,
                Id = track.Id,
                Name = track.Name,
                TotalSeconds = totalSeconds
            });
        }

        return response;
    }

    


    [Authorize]
    [HttpGet("popular")]
    public async Task<ActionResult<List<TrackGetResponseToShow>>> GetPopularByMusician(
        [FromQuery] Guid musicianId,
        CancellationToken ct,
        int takeCount = 3)
    {
        var popularTracks = await _trackRepository.GetPopularByMusicianId(musicianId, takeCount, ct);
        var response = new List<TrackGetResponseToShow>();
        foreach (var track in popularTracks)
        {
            var fileBytes = await _fileService.ReadFile(track.FilePath, ct);
            var totalSeconds = await _trackService.GetTotalDurationOfFileInSeconds(fileBytes);
            response.Add(new TrackGetResponseToShow()
            {
                MusicianId = track.User.Id,
                Id = track.Id,
                Name = track.Name,
                AuthorName = track.User.Name,
                TotalSeconds = totalSeconds
            });
        }

        return response;
    }
}