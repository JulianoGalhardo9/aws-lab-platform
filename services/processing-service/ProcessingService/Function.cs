using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ProcessingService;

public class Function
{
    public Function()
    {
    }

    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach (var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        context.Logger.LogInformation($"Processando mensagem ID: {message.MessageId}");

        try
        {
            var fileEvent = JsonSerializer.Deserialize<FileUploadedEventBody>(message.Body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (fileEvent != null)
            {
                context.Logger.LogInformation($"[SUCESSO] Arquivo detectado para processamento!");
                context.Logger.LogInformation($"ID do Arquivo: {fileEvent.FileId}");
                context.Logger.LogInformation($"S3 Key de Origem: {fileEvent.S3Key}");
                context.Logger.LogInformation($"Tamanho do Arquivo: {fileEvent.SizeInBytes} bytes");
                context.Logger.LogInformation($"Pertence ao Usuário ID: {fileEvent.UserId}");

                await Task.Delay(100);
            }
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"[ERRO] Falha ao processar a mensagem {message.MessageId}. Detalhes: {ex.Message}");
            throw;
        }
    }
}

public record FileUploadedEventBody(
    Guid FileId, 
    string S3Key, 
    string FileName, 
    string ContentType, 
    long SizeInBytes, 
    Guid UserId
);