using UploadService.Application.Events;

namespace UploadService.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync(FileUploadedEvent @event, CancellationToken cancellationToken);
}