using CoachBoard.Application.Interfaces;
using CoachBoard.Application.Models;
using Microsoft.Extensions.Options;

namespace CoachBoard.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _options;

    public LocalFileStorageService(IOptions<FileStorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> SaveAsync(Stream content, string originalFileName, string contentType, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var rootPath = GetRootPath();
        Directory.CreateDirectory(rootPath);

        var filePath = Path.Combine(rootPath, fileName);
        await using (var output = File.Create(filePath))
        {
            await content.CopyToAsync(output, cancellationToken);
        }

        return $"{_options.PublicBasePath.TrimEnd('/')}/{fileName}";
    }

    public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(fileUrl);
        if (string.IsNullOrWhiteSpace(fileName)) return Task.CompletedTask;

        var filePath = Path.Combine(GetRootPath(), fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    private string GetRootPath()
    {
        return Path.IsPathRooted(_options.RootPath)
            ? _options.RootPath
            : Path.Combine(Directory.GetCurrentDirectory(), _options.RootPath);
    }
}
