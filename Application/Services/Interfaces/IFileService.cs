using Microsoft.AspNetCore.Mvc;

namespace Application.Services.Interfaces;

public interface IFileService
{
    Task<byte[]> ReadFile(string path, CancellationToken ct);
    Task<string> WriteFile(IFormFile file, Guid id, string directory, CancellationToken ct);
}