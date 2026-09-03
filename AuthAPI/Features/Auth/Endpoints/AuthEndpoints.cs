using AuthAPI.Features.Auth.DTOs;
using AuthAPI.Services;
using Serilog;

namespace AuthAPI.Features.Auth.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var authGroup = app.MapGroup("/auth");
            
            authGroup.MapPost("/register", async (RegisterRequest request, IAuthService authService) =>
            {
                Log.Information("Recebida requisição de registro para usuário: {Username}", request.Username);
                var result = await authService.RegisterAsync(request);
                if (result)
                {
                    Log.Information("Registro bem-sucedido para usuário: {Username}", request.Username);
                    return Results.Ok(new { message = "Usuário registrado com sucesso!" });
                }
                else
                {
                    Log.Warning("Falha no registro para usuário: {Username}, nome de usuário ou email já existente", request.Username);
                    return Results.BadRequest(new { message = "Nome de usuário ou email já existente." });
                }
            });

            authGroup.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
            {
                Log.Information("Recebida requisição de login para usuário: {Username}", request.Username);
                var response = await authService.LoginAsync(request);
                if (response != null)
                {
                    Log.Information("Login bem-sucedido para usuário: {Username}", request.Username);
                    return Results.Ok(response);
                }
                else
                {
                    Log.Warning("Falha no login para usuário: {Username}, credenciais inválidas", request.Username);
                    return Results.Unauthorized();
                }
            });
        }
    }
}