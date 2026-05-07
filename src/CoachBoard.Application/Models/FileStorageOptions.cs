namespace CoachBoard.Application.Models;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string RootPath { get; set; } = "wwwroot/uploads/progress";
    public string PublicBasePath { get; set; } = "/uploads/progress";
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } = ["jpg", "jpeg", "png", "webp"];
}
