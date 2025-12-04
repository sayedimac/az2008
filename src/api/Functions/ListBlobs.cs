using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Api.Functions;

public class ListBlobs
{
    private readonly ILogger<ListBlobs> _logger;

    public ListBlobs(ILogger<ListBlobs> logger)
    {
        _logger = logger;
    }

    [Function("ListBlobs")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "blobs/{containerName}")] HttpRequest req,
        string containerName)
    {
        _logger.LogInformation("ListBlobs function processing request for container: {ContainerName}", containerName);

        try
        {
            var connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("StorageConnectionString environment variable is not set");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var blobs = new List<BlobInfo>();
            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                blobs.Add(new BlobInfo
                {
                    Name = blobItem.Name,
                    Size = blobItem.Properties.ContentLength ?? 0,
                    ContentType = blobItem.Properties.ContentType ?? "unknown",
                    LastModified = blobItem.Properties.LastModified?.UtcDateTime
                });
            }

            _logger.LogInformation("Found {Count} blobs in container {ContainerName}", blobs.Count, containerName);
            return new OkObjectResult(blobs);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Container {ContainerName} not found", containerName);
            return new NotFoundObjectResult($"Container '{containerName}' not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing blobs in container {ContainerName}", containerName);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}

public class BlobInfo
{
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime? LastModified { get; set; }
}
