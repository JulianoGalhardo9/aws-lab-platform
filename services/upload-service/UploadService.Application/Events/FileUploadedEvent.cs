namespace UploadService.Application.Events;
public record FileUploadedEvent(
    Guid FileId, 
    string S3Key, 
    string FileName, 
    string ContentType, 
    long SizeInBytes, 
    Guid UserId
);