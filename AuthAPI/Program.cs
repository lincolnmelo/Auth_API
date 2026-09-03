using AuthAPI.Models;
using AuthAPI.Services;
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

// Rotas de autenticação
app.MapPost("/auth/register", async (RegisterRequest request, IAuthService authService) =>
{
    var result = await authService.RegisterAsync(request);
    return result ? Results.Ok(new { message = "Usuário registrado com sucesso!" }) 
                  : Results.BadRequest(new { message = "Nome de usuário ou email já existente." });
});

app.MapPost("/auth/login", async (LoginRequest request, IAuthService authService) =>
{
    var response = await authService.LoginAsync(request);
    return response != null ? Results.Ok(response) 
                            : Results.Unauthorized();
});

// Rota protegida para testar autenticação
app.MapGet("/api/protected", () =>
{
    return Results.Ok(new { message = "Acesso autorizado!" });
})
.RequireAuthorization();

app.Run();