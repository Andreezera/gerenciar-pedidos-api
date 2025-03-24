using GerenciarPedidos.API.Middlewares;
using GerenciarPedidos.Data.Interfaces;
using GerenciarPedidos.Data.Repositories;
using GerenciarPedidos.Domain.Interfaces;
using GerenciarPedidos.Domain.Services;
using GerenciarPedidos.Domain.Services.CalculoImposto;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}")
    .CreateLogger();

builder.Host.UseSerilog(Log.Logger);

builder.Logging.ClearProviders();
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Error);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GerenciarPedidos API",
        Version = "v1",
        Description = "API para gerenciamento de pedidos"
    });
});

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<FeatureFlagService>();
builder.Services.AddSingleton<CalculoImpostoFactory>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<ICalculoImpostoFactory, CalculoImpostoFactory>();
builder.Services.AddTransient<ICalculoImpostoService, CalculoImpostoVigente>();
builder.Services.AddTransient<CalculoImpostoVigente>();
builder.Services.AddTransient<CalculoImpostoReforma>();
builder.Services.AddSingleton<IPedidoRepository, PedidoRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GerenciarPedidos API v1");
    c.RoutePrefix = string.Empty;
});

app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, _, ex) =>
        ex != null ? LogEventLevel.Error : LogEventLevel.Information;
});
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthorization();
app.MapControllers();

Log.Information("GerenciarPedidosAPI iniciado com sucesso!");
Log.Information("Data/Hora de Inicialização: {DataHora}", DateTime.Now);

app.Run();