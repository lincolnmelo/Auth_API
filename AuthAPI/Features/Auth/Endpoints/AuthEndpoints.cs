using AuthAPI.Features.Auth.DTOs;
using AuthAPI.Services;

namespace AuthAPI.Features.Auth.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var authGroup = app.MapGroup("/auth");
            
            authGroup.MapPost("/register", async (RegisterRequest request, IAuthService authService) =>
            {
                var result = await authService.RegisterAsync(request);
                return result ? Results.Ok(new { message = "Usuário registrado com sucesso!" }) 
                              : Results.BadRequest(new { message = "Nome de usuário ou email já existente." });
            });

            authGroup.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
            {
                var response = await authService.LoginAsync(request);
                return response != null ? Results.Ok(response) 
                                        : Results.Unauthorized();
            });
        }
    }
}