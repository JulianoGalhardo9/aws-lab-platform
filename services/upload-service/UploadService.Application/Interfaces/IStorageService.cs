namespace UploadService.Application.Interfaces;

public interface IStorageService
{
    string GeneratePresignedUrl(string s3Key, string contentType, TimeSpan expiration);
}