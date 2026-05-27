using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using UploadService.Application.Events;
using UploadService.Application.Interfaces;

namespace UploadService.Infrastructure.Messaging;

public class SqsPublisher : IEventPublisher
{
    private readonly IAmazonSQS _sqsClient;
    
    private const string QueueUrl = "http://localhost:4566/000000000000/file-uploaded-queue";

    public SqsPublisher(IAmazonSQS sqsClient)
    {
        _sqsClient = sqsClient;
    }

    public async Task PublishAsync(FileUploadedEvent @event, CancellationToken cancellationToken)
    {
        var messageBody = JsonSerializer.Serialize(@event);

        var request = new SendMessageRequest
        {
            QueueUrl = QueueUrl,
            MessageBody = messageBody
        };

        await _sqsClient.SendMessageAsync(request, cancellationToken);
    }
}