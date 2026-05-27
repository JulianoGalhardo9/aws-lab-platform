using Amazon.S3;
using Amazon.S3.Model;
using UploadService.Application.Interfaces;

namespace UploadService.Infrastructure.Storage;

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private const string BucketName = "aws-lab-platform-bucket";

    public S3StorageService(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    public string GeneratePresignedUrl(string s3Key, string contentType, TimeSpan expiration)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = BucketName,
            Key = s3Key,
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = DateTime.UtcNow.Add(expiration)
        };

        return _s3Client.GetPreSignedURL(request);
    }
}