using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;

namespace AuthAPI.Middlewares;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Obter ou gerar o Correlation ID
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
        
        // Se não existir, gerar um novo Guid
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }
        
        // Adicionar o Correlation ID no header de resposta
        context.Response.Headers.Append("X-Correlation-ID", correlationId);
        
        // Adicionar o Correlation ID ao escopo de log do Serilog
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            Log.Information("Middleware executado com CorrelationId: {CorrelationId}", correlationId);
            await _next(context);
        }
    }
}