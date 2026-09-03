using Microsoft.EntityFrameworkCore;
using Serilog;

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
    // Adicionando configurações específicas para Swagger
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Auth API",
        Version = "v1",
        Description = "API de autenticação"
    });
});

// Configurar o DbContext com MariaDB
builder.Services.AddDbContext<AuthAPI.Infrastructure.Data.AuthContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(10, 5, 12)) // Versão do MariaDB
    ));

// Registrar serviços
builder.Services.AddScoped<AuthAPI.Services.IAuthService, AuthAPI.Services.AuthService>();

// Configurar o Serilog para o host
builder.Host.UseSerilog((context, configuration) => configuration
    .WriteTo.Console()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information));

var app = builder.Build();

Log.Information("Configurando pipeline HTTP da aplicação");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1");
        options.RoutePrefix = "swagger"; // Swagger será acessado em /swagger
        options.DisplayRequestDuration();
        // Adicionando configurações adicionais para evitar problemas de CORS
        options.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
    });
}

app.UseHttpsRedirection();

// Mapear endpoints da autenticação
AuthAPI.Features.Auth.Endpoints.AuthEndpoints.MapAuthEndpoints(app);

// Rota protegida para testar autenticação
app.MapGet("/api/protected", () =>
{
    return Results.Ok(new { message = "Acesso autorizado!" });
})
.RequireAuthorization();

Log.Information("Aplicação Auth API está pronta para receber requisições");

app.Run();