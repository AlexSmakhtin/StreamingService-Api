namespace Domain.Services.Interfaces;

public interface ITrackService
{
    Task<double> GetTotalDurationOfFileInSeconds(byte[] array);
}