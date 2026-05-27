using MediatR;

namespace UploadService.Application.UseCases.Files.Commands.ConfirmUpload;

public record ConfirmUploadCommand(Guid FileId) : IRequest;