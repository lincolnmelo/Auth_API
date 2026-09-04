using AuthAPI.Infrastructure.Data;
using AuthAPI.Models.Entities;
using AuthAPI.Features.Auth.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace AuthAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AuthContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Username);

            if (user == null)
                return null;

            // Verificar senha (em produção, use um método mais seguro como BCrypt)
            if (!VerifyPassword(request.Password, user.PasswordHash))
                return null;

            // Gerar token JWT
            var token = GenerateJwtToken(user);

            return new LoginResponse
            {
                Token = token,
                Username = user.Username,
                Email = user.Email
            };
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            // Verificar se usuário já existe
            if (await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
                return false;

            // Criptografar senha
            var passwordHash = HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }

        private string GenerateJwtToken(User user)
        {
            // Configurar o token JWT
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "User") // Pode ser ajustado conforme necessário
            };

            // Obter a chave secreta do appsettings.json
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            
            // Se não encontrar a chave no appsettings, usar uma chave padrão (não recomendado para produção)
            if (string.IsNullOrEmpty(secretKey))
            {
                secretKey = "Chave_Secreta_Padrao_Muito_Longa_E_Complexa_1234567890";
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Configurar a expiração do token (por exemplo, 1 hora)
            var expireInMinutes = Environment.GetEnvironmentVariable("JWT_EXPIRE_IN_MINUTES");
            int expireMinutes = string.IsNullOrEmpty(expireInMinutes) ? 60 : int.Parse(expireInMinutes);
            
            var token = new JwtSecurityToken(
                issuer: Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "AuthAPI",
                audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "AuthAPIUsers",
                claims: claims,
                expires: DateTime.Now.AddMinutes(expireMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}