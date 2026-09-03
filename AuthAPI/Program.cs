using AuthAPI.Infrastructure.Data;
using AuthAPI.Services;
using AuthAPI.Features.Auth.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar o DbContext com MariaDB
builder.Services.AddDbContext<AuthContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(10, 5, 12)) // Versão do MariaDB
    ));

// Registrar serviços
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Mapear endpoints de autenticação
app.MapAuthEndpoints();

// Rota protegida para testar autenticação
app.MapGet("/api/protected", () =>
{
    return Results.Ok(new { message = "Acesso autorizado!" });
})
.RequireAuthorization();

app.Run();