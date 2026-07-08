using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace LarCooperativa.Api.RateLimiting;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var api = configuration.GetSection("RateLimiting:Api").Get<ApiRateLimitSettings>() ?? new();
        var login = configuration.GetSection("RateLimiting:Login").Get<LoginRateLimitSettings>() ?? new();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, _) =>
            {
                // O limiter global (sliding window encadeado) não propaga a metadata RetryAfter;
                // nesse caso usamos a própria janela como estimativa conservadora
                var retryAfterSegundos = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                    ? (int)Math.Ceiling(retryAfter.TotalSeconds)
                    : api.WindowSeconds;
                context.HttpContext.Response.Headers.RetryAfter = retryAfterSegundos.ToString();

                context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("LarCooperativa.Api.RateLimiting")
                    .LogWarning("Requisição rejeitada por rate limiting em {Path}", context.HttpContext.Request.Path);

                return ValueTask.CompletedTask;
            };

            // Aplica-se a TODOS os endpoints, sem precisar de atributo por controller;
            // endpoints novos já nascem limitados
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    ChaveDoUsuario(context) ?? ChaveDoCliente(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = api.PermitLimit,
                        Window = TimeSpan.FromSeconds(api.WindowSeconds),
                        SegmentsPerWindow = api.SegmentsPerWindow,
                        QueueLimit = 0,
                    }));

            // Política adicional (encadeada ao limite global) para o endpoint anônimo de login
            options.AddPolicy(RateLimitPolicies.Login, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    ChaveDoCliente(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = login.PermitLimit,
                        Window = TimeSpan.FromSeconds(login.WindowSeconds),
                        QueueLimit = 0,
                    }));
        });

        return services;
    }

    private static string? ChaveDoUsuario(HttpContext context) =>
        context.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? context.User.FindFirstValue("sub");

    private static string ChaveDoCliente(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "cliente-desconhecido";
}
