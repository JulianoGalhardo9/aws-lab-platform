using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.SimpleNotificationService;
using System.Text.Json;

namespace ProcessingService.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IAmazonSQS _sqsClient;
    private readonly IAmazonSimpleNotificationService _snsClient;
    
    private const string QueueUrl = "http://localhost:4566/000000000000/file-uploaded-queue";
    private const string SnsTopicArn = "arn:aws:sns:us-east-1:000000000000:file-processed-topic";

    public Worker(ILogger<Worker> logger, IAmazonSQS sqsClient, IAmazonSimpleNotificationService snsClient)
    {
        _logger = logger;
        _sqsClient = sqsClient;
        _snsClient = snsClient;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker de Processamento iniciado no ECS Fargate...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var receiveRequest = new ReceiveMessageRequest
                {
                    QueueUrl = QueueUrl,
                    MaxNumberOfMessages = 5,
                    WaitTimeSeconds = 20 
                };

                var response = await _sqsClient.ReceiveMessageAsync(receiveRequest, stoppingToken);

                foreach (var message in response.Messages)
                {
                    _logger.LogInformation($"[Worker] Mensagem capturada de forma contínua: {message.MessageId}");

                    await Task.Delay(2000, stoppingToken);

                    _logger.LogInformation($"[Worker] Processamento concluído com sucesso para a mensagem {message.MessageId}!");

                    var notificationMessage = new { FileId = Guid.NewGuid(), Status = "Completed", Message = "Arquivo processado pelo Worker Fargate" };
                    await _snsClient.PublishAsync(SnsTopicArn, JsonSerializer.Serialize(notificationMessage), stoppingToken);

                    await _sqsClient.DeleteMessageAsync(QueueUrl, message.ReceiptHandle, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro no ciclo de consumo do Worker: {ex.Message}");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}