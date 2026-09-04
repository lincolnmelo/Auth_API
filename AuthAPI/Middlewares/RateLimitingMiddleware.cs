using Microsoft.AspNetCore.Http;
using Serilog;
using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace AuthAPI.Middlewares;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();
    private readonly Serilog.ILogger _logger;

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
        _logger = Log.ForContext<RateLimitingMiddleware>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Verificar se o rate limiting está habilitado
        var rateLimitEnabled = bool.TryParse(Environment.GetEnvironmentVariable("RATE_LIMIT_ENABLED"), out var enabled) && enabled;
        if (!rateLimitEnabled)
        {
            await _next(context);
            return;
        }

        // Obter configurações do rate limiting
        var windowMinutes = int.Parse(Environment.GetEnvironmentVariable("RATE_LIMIT_WINDOW_MINUTES") ?? "5");
        var requestsPerWindow = int.Parse(Environment.GetEnvironmentVariable("RATE_LIMIT_REQUESTS_PER_WINDOW") ?? "10");

        // Criar uma chave única para o cliente (usando IP)
        var clientKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // Obter ou criar bucket para este cliente
        var bucket = _buckets.GetOrAdd(clientKey, _ => new TokenBucket(requestsPerWindow, TimeSpan.FromMinutes(windowMinutes)));

        // Tentar consumir um token
        if (bucket.TryConsume(1))
        {
            _logger.Information("Requisição permitida para cliente {ClientKey}", clientKey);
            await _next(context);
        }
        else
        {
            _logger.Warning("Requisição bloqueada por rate limiting para cliente {ClientKey}", clientKey);
            context.Response.StatusCode = 429; // Too Many Requests
            await context.Response.WriteAsync("Limite de requisições excedido. Tente novamente mais tarde.");
        }
    }

    private class TokenBucket
    {
        private readonly int _tokens;
        private readonly TimeSpan _refillInterval;
        private int _availableTokens;
        private DateTime _lastRefill;

        public TokenBucket(int tokens, TimeSpan refillInterval)
        {
            _tokens = tokens;
            _refillInterval = refillInterval;
            _availableTokens = tokens;
            _lastRefill = DateTime.UtcNow;
        }

        public bool TryConsume(int tokens)
        {
            Refill();
            
            if (_availableTokens >= tokens)
            {
                _availableTokens -= tokens;
                return true;
            }
            
            return false;
        }

        private void Refill()
        {
            var now = DateTime.UtcNow;
            var elapsed = now - _lastRefill;
            
            if (elapsed > _refillInterval)
            {
                _availableTokens = Math.Min(_tokens, _availableTokens + (int)(elapsed.TotalMinutes / _refillInterval.TotalMinutes) * _tokens);
                _lastRefill = now;
            }
        }
    }
}