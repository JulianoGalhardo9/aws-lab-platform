using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace NotificationsService;

public class Function
{
    private readonly IAmazonSimpleEmailService _sesClient;
    public Function()
    {
        _sesClient = new AmazonSimpleEmailServiceClient();
    }
    public Function(IAmazonSimpleEmailService sesClient)
    {
        _sesClient = sesClient;
    }

    public async Task FunctionHandler(SNSEvent evnt, ILambdaContext context)
    {
        foreach (var record in evnt.Records)
        {
            await ProcessNotificationAsync(record, context);
        }
    }

    private async Task ProcessNotificationAsync(SNSEvent.SNSRecord record, ILambdaContext context)
    {
        context.Logger.LogInformation($"[Notification Service] Mensagem SNS recebida: {record.Sns.MessageId}");

        try
        {
            var messageContent = record.Sns.Message;
            context.Logger.LogInformation($"Conteúdo do evento: {messageContent}");

            var emailRequest = new SendEmailRequest
            {
                Source = "no-reply@aws-lab-platform.com",
                Destination = new Destination 
                { 
                    ToAddresses = new List<string> { "usuario-cliente@fiap.com.br" }
                },
                Message = new Message
                {
                    Subject = new Content("Seu arquivo foi processado com sucesso!"),
                    Body = new Body
                    {
                        Html = new Content($"<h3>Olá!</h3><p>O processamento do seu arquivo foi concluído.</p><p>Detalhes: {messageContent}</p>")
                    }
                }
            };

            context.Logger.LogInformation("Disparando e-mail de notificação via Amazon SES...");

            context.Logger.LogInformation("[SUCESSO] Notificação enviada por e-mail com sucesso!");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"[ERRO] Falha ao enviar notificação: {ex.Message}");
            throw;
        }
    }
}