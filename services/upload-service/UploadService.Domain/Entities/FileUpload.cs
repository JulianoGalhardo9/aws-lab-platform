using UploadService.Domain.Enums;

namespace UploadService.Domain.Entities;

public class FileUpload
{
    public Guid Id { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeInBytes { get; private set; }
    public string S3Key { get; private set; } = string.Empty;
    public UploadStatus Status { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected FileUpload() { }

    public FileUpload(string fileName, string contentType, long sizeInBytes, Guid userId)
    {
        Id = Guid.NewGuid();
        FileName = fileName;
        ContentType = contentType;
        SizeInBytes = sizeInBytes;
        UserId = userId;
        Status = UploadStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        
        S3Key = $"uploads/{userId}/{Id}-{fileName}";
    }

    public void UpdateStatus(UploadStatus newStatus)
    {
        Status = newStatus;
    }
}