using System.Net;
using System.Net.Http.Json;
using LarCooperativa.Api.Contracts;
using Microsoft.AspNetCore.Hosting;

namespace LarCooperativa.IntegrationTests;

/// <summary>
/// Fábrica própria com limites baixos para exercitar o throttling; cada teste usa uma
/// instância nova para ter contadores zerados, sem interferir nos demais testes da suíte.
/// </summary>
public sealed class RateLimitingApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("RateLimiting:Login:PermitLimit", "2");
        builder.UseSetting("RateLimiting:Login:WindowSeconds", "60");
        builder.UseSetting("RateLimiting:Api:PermitLimit", "3");
        builder.UseSetting("RateLimiting:Api:WindowSeconds", "60");
    }
}

public class RateLimitingTests
{
    [Fact]
    public async Task Login_AoExcederOLimitePorIp_Retorna429ComRetryAfter()
    {
        await using var factory = new RateLimitingApiFactory();
        var client = factory.CreateClient();
        var request = new LoginRequest { Usuario = "qualquer", Senha = "qualquer" };

        await client.PostAsJsonAsync("/api/auth/login", request);
        await client.PostAsJsonAsync("/api/auth/login", request);
        var excedente = await client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.TooManyRequests, excedente.StatusCode);
        Assert.NotNull(excedente.Headers.RetryAfter);
    }

    [Fact]
    public async Task Api_AoExcederOLimitePorUsuario_Retorna429()
    {
        await using var factory = new RateLimitingApiFactory();
        var client = await factory.CreateAuthenticatedClientAsync();

        var dentroDoLimite = new[]
        {
            await client.GetAsync("/api/pessoas"),
            await client.GetAsync("/api/pessoas"),
            await client.GetAsync("/api/pessoas"),
        };
        var excedente = await client.GetAsync("/api/pessoas");

        Assert.All(dentroDoLimite, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        Assert.Equal(HttpStatusCode.TooManyRequests, excedente.StatusCode);
        Assert.NotNull(excedente.Headers.RetryAfter);
    }

    [Fact]
    public async Task Login_DentroDoLimite_NaoEhLimitado()
    {
        await using var factory = new RateLimitingApiFactory();
        var client = factory.CreateClient();
        var request = new LoginRequest { Usuario = "qualquer", Senha = "qualquer" };

        var primeira = await client.PostAsJsonAsync("/api/auth/login", request);
        var segunda = await client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, primeira.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, segunda.StatusCode);
    }
}
