using Microsoft.Extensions.Hosting;
using SareeGrace.Application.Interfaces;

namespace SareeGrace.Infrastructure.Services;

/// <summary>
/// Local file-based image storage. Admin uploads images through the panel — no code changes needed.
/// In Phase B (Azure), this will be swapped with AzureBlobImageService via DI.
/// </summary>
public class LocalImageService : IImageService
{
    private readonly string _basePath;

    public LocalImageService(IHostEnvironment env)
    {
        // WebRootPath is not available on IHostEnvironment; use wwwroot under ContentRootPath
        var webRootPath = Path.Combine(env.ContentRootPath, "wwwroot");
        _basePath = Path.Combine(webRootPath, "images");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveImageAsync(Stream imageStream, string fileName, string folder = "products")
    {
        var folderPath = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(folderPath);

        // Generate unique filename
        var extension = Path.GetExtension(fileName);
        var uniqueName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(folderPath, uniqueName);

        using var fileStream = new FileStream(filePath, FileMode.Create);
        await imageStream.CopyToAsync(fileStream);

        // Return relative URL path for serving via static files
        return $"/images/{folder}/{uniqueName}";
    }

    public bool DeleteImage(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath)) return false;

        // Convert URL path to file path
        var relativePath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(Directory.GetParent(_basePath)!.FullName, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return true;
        }
        return false;
    }
}
