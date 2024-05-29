using Application.Services.Interfaces;

namespace Application.Services.Implementations;

public class FileService : IFileService
{
    private readonly SemaphoreSlim _semaphoreSlim;

    public FileService()
    {
        _semaphoreSlim = new SemaphoreSlim(1);
    }

    public async Task<byte[]> ReadFile(string path, CancellationToken ct)
    {
        var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), path);
        await _semaphoreSlim.WaitAsync(ct);
        var fileBytes = await File.ReadAllBytesAsync(absolutePath, ct);
        _semaphoreSlim.Release();
        return fileBytes;
    }

    public async Task<string> WriteFile(IFormFile file, Guid id, string directory, CancellationToken ct)
    {
        var filePath = Path.Combine(directory, $"{id}_{file.FileName}");
        var absoluteFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
        await _semaphoreSlim.WaitAsync(ct);
        await using (var fs = new FileStream(absoluteFilePath, FileMode.Create))
        {
            await file.CopyToAsync(fs, ct);
        }
        _semaphoreSlim.Release();
        return filePath;
    }
}