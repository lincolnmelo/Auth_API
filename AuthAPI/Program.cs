using Microsoft.EntityFrameworkCore;
using Serilog;
using DotNetEnv;
using System.IO;
using AuthAPI.Middlewares;

// Verificar se o arquivo .env.local existe antes de carregar as vari�veis de ambiente
var envLocalPath = Path.Combine(Directory.GetCurrentDirectory(), ".env.local");
if (File.Exists(envLocalPath))
{
    // Carregar vari�veis de ambiente do .env.local
    Env.Load(envLocalPath);
}

var builder = WebApplication.CreateBuilder(args);

// Configurar o Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Iniciando aplicação Auth API");

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Adicionando configura��es espec�ficas para Swagger
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Auth API",
        Version = "v1",
        Description = "API de autenticação"
    });
});

// Configurar o DbContext com MariaDB usando vari�veis do .env.local
var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRING_DB");
builder.Services.AddDbContext<AuthAPI.Infrastructure.Data.AuthContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(10, 5, 12)) // Vers�o do MariaDB
    ));

// Registrar servi�os
builder.Services.AddScoped<AuthAPI.Services.IAuthService, AuthAPI.Services.AuthService>();

// Configurar o Serilog para o host
builder.Host.UseSerilog((context, configuration) => configuration
    .WriteTo.Console()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information));

var app = builder.Build();

Log.Information("Configurando pipeline HTTP da aplica��o");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1");
        options.RoutePrefix = "swagger"; // Swagger ser� acessado em /swagger
        options.DisplayRequestDuration();
        // Adicionando configura��es adicionais para evitar problemas de CORS
        options.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
    });
}

app.UseHttpsRedirection();

// Usar os middlewares personalizados
app.UseCorrelationId();
app.UseRateLimiting();

// Mapear endpoints da autenticação
AuthAPI.Features.Auth.Endpoints.AuthEndpoints.MapAuthEndpoints(app);

// Rota protegida para testar autenticação
app.MapGet("/api/protected", () =>
{
    return Results.Ok(new { message = "Acesso autorizado!" });
})
.RequireAuthorization();

Log.Information("Aplica��o Auth API est� pronta para receber requisi��es");

app.Run();
