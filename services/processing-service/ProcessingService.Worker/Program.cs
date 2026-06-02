using Amazon.SQS;
using Amazon.SimpleNotificationService;
using ProcessingService.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IAmazonSQS>(sp => new AmazonSQSClient(new AmazonSQSConfig
{
    ServiceURL = "http://localhost:4566"
}));

builder.Services.AddSingleton<IAmazonSimpleNotificationService>(sp => new AmazonSimpleNotificationServiceClient(new AmazonSimpleNotificationServiceConfig
{
    ServiceURL = "http://localhost:4566"
}));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();