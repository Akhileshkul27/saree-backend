using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using SareeGrace.Application.Interfaces;

namespace SareeGrace.Infrastructure.Services;

/// <summary>
/// Azure Blob Storage image service — replaces LocalImageService for cloud deployments.
/// Images are stored in Azure Blob Storage and served directly via their public blob URL.
/// Configure via AzureStorage:ConnectionString and AzureStorage:ContainerName in appsettings.
/// </summary>
public class AzureBlobImageService : IImageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly string _containerBaseUrl;

    public AzureBlobImageService(IConfiguration config)
    {
        var connStr = config["AzureStorage:ConnectionString"]
            ?? throw new InvalidOperationException("AzureStorage:ConnectionString is not configured.");

        var containerName = config["AzureStorage:ContainerName"] ?? "product-images";

        _containerClient = new BlobContainerClient(connStr, containerName);
        _containerClient.CreateIfNotExists(PublicAccessType.Blob);

        // Base URL used to construct readable image URLs
        // e.g. https://sareegraceimages.blob.core.windows.net/product-images
        _containerBaseUrl = _containerClient.Uri.ToString().TrimEnd('/');
    }

    public async Task<string> SaveImageAsync(Stream imageStream, string fileName, string folder = "products")
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var blobName = $"{folder}/{Guid.NewGuid():N}{extension}";

        var blobClient = _containerClient.GetBlobClient(blobName);

        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"            => "image/png",
            ".webp"           => "image/webp",
            _                 => "application/octet-stream"
        };

        await blobClient.UploadAsync(imageStream, new BlobHttpHeaders { ContentType = contentType });

        // Return full public URL — stored in DB and used directly by frontend
        return $"{_containerBaseUrl}/{blobName}";
    }

    public bool DeleteImage(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return false;
        try
        {
            // Extract blob name from full URL:
            // "https://sareegraceimages.blob.core.windows.net/product-images/products/abc.jpg"
            //  → blob name = "products/abc.jpg"
            var prefix = _containerBaseUrl + "/";
            if (!imageUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;

            var blobName = imageUrl[prefix.Length..];
            var blobClient = _containerClient.GetBlobClient(blobName);
            blobClient.DeleteIfExists();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
