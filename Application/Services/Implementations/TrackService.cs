using Domain.Services.Interfaces;
using NAudio.Wave;

namespace Application.Services.Implementations;

public class TrackService : ITrackService
{
    public async Task<double> GetTotalDurationOfFileInSeconds(byte[] array)
    {
        await using var ms = new MemoryStream(array);
        await using var mp3 = new Mp3FileReader(ms);
        return mp3.TotalTime.TotalSeconds;
    }
}