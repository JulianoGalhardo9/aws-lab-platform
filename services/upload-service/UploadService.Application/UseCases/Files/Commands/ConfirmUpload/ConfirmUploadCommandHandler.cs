using MediatR;
using UploadService.Application.Events;
using UploadService.Application.Interfaces;
using UploadService.Domain.Enums;

namespace UploadService.Application.UseCases.Files.Commands.ConfirmUpload;

public class ConfirmUploadCommandHandler : IRequestHandler<ConfirmUploadCommand>
{
    private readonly IEventPublisher _eventPublisher;

    public ConfirmUploadCommandHandler(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public async Task Handle(ConfirmUploadCommand request, CancellationToken cancellationToken)
    {

        var fileUploadedEvent = new FileUploadedEvent(
            FileId: request.FileId,
            S3Key: $"uploads/user-fake-123/{request.FileId}-documento.pdf",
            FileName: "documento.pdf",
            ContentType: "application/pdf",
            SizeInBytes: 1048576,
            UserId: Guid.NewGuid()
        );

        await _eventPublisher.PublishAsync(fileUploadedEvent, cancellationToken);
    }
}