using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Serilog;
using Serilog.Events;
using AuthService.Application.Interfaces;
using AuthService.Application.UseCases.Users.Commands.Login;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .CreateLogger();

try
{
    Log.Information("Iniciando o Auth Service...");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<AuthDbContext>(options =>
        options.UseSqlServer(connectionString));

    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddSingleton<ITokenService, TokenService>();
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly));
    builder.Services.AddValidatorsFromAssembly(typeof(LoginCommandValidator).Assembly);

    var app = builder.Build();

    app.UseHttpsRedirection();

    app.MapPost("/api/auth/login", async (LoginCommand command, MediatR.IMediator mediator, IValidator<LoginCommand> validator) =>
    {
        var validationResult = await validator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            Log.Warning("Falha de validação no Login para o e-mail: {Email}", command.Email);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var response = await mediator.Send(command);
            Log.Information("Login realizado com sucesso para o e-mail: {Email}", command.Email);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Warning("Tentativa de login não autorizada: {Email}", command.Email);
            return Results.Json(new { error = ex.Message }, statusCode: 401);
        }
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "O Auth Service falhou ao iniciar de forma catastrófica.");
}
finally
{
    Log.CloseAndFlush();
}