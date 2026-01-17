using Minio;
using Minio.DataModel.Args;
using Microsoft.Extensions.Options;

namespace ResearchPlatform.Api.Services;

public class StorageSettings
{
    public required string Endpoint { get; set; }
    public string? ExternalEndpoint { get; set; } // External URL for presigned URLs (accessible from browser)
    public required string AccessKey { get; set; }
    public required string SecretKey { get; set; }
    public required string BucketName { get; set; }
    public bool UseSSL { get; set; } = false;
}

public class StorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;
    private readonly string _internalEndpoint;
    private readonly string? _externalEndpoint;
    private readonly ILogger<StorageService> _logger;

    public StorageService(IOptions<StorageSettings> settings, ILogger<StorageService> logger)
    {
        var config = settings.Value;
        _bucketName = config.BucketName;
        _internalEndpoint = config.Endpoint;
        _externalEndpoint = config.ExternalEndpoint;
        _logger = logger;

        _minioClient = new MinioClient()
            .WithEndpoint(config.Endpoint)
            .WithCredentials(config.AccessKey, config.SecretKey)
            .WithSSL(config.UseSSL)
            .Build();
    }

    public async Task EnsureBucketExistsAsync()
    {
        try
        {
            var beArgs = new BucketExistsArgs()
                .WithBucket(_bucketName);
            bool found = await _minioClient.BucketExistsAsync(beArgs);

            if (!found)
            {
                var mbArgs = new MakeBucketArgs()
                    .WithBucket(_bucketName);
                await _minioClient.MakeBucketAsync(mbArgs);
                _logger.LogInformation($"Created bucket: {_bucketName}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error ensuring bucket exists: {_bucketName}");
            throw;
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string objectName, string contentType)
    {
        try
        {
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);
            _logger.LogInformation($"Uploaded file: {objectName}");

            return objectName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading file: {objectName}");
            throw;
        }
    }

    public async Task<string> GetPresignedUrlAsync(string objectName, int expirySeconds = 3600)
    {
        try
        {
            var args = new PresignedGetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithExpiry(expirySeconds);

            string url = await _minioClient.PresignedGetObjectAsync(args);

            // Replace internal endpoint with external endpoint for browser access
            if (!string.IsNullOrEmpty(_externalEndpoint))
            {
                url = url.Replace(_internalEndpoint, _externalEndpoint);
            }

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating presigned URL for: {objectName}");
            throw;
        }
    }

    public async Task<Stream> GetFileStreamAsync(string objectName)
    {
        try
        {
            var memoryStream = new MemoryStream();
            var args = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithCallbackStream((stream) =>
                {
                    stream.CopyTo(memoryStream);
                });

            await _minioClient.GetObjectAsync(args);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting file stream for: {objectName}");
            throw;
        }
    }

    public async Task DeleteFileAsync(string objectName)
    {
        try
        {
            var args = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName);

            await _minioClient.RemoveObjectAsync(args);
            _logger.LogInformation($"Deleted file: {objectName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting file: {objectName}");
            throw;
        }
    }
}
