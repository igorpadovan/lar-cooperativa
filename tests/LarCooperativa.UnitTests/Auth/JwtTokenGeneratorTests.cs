using LarCooperativa.Api.Auth;
using LarCooperativa.Api.Domain;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;

namespace LarCooperativa.UnitTests.Auth;

public class JwtTokenGeneratorTests
{
    private static readonly DateTimeOffset Agora = new(2026, 7, 7, 12, 0, 0, TimeSpan.Zero);

    private static readonly JwtSettings Settings = new()
    {
        Key = "chave-de-testes-com-tamanho-suficiente-para-hmac-sha256",
        Issuer = "LarCooperativa",
        Audience = "LarCooperativa.Api",
        ExpirationMinutes = 60,
    };

    private readonly JwtTokenGenerator _generator =
        new(Options.Create(Settings), new FakeTimeProvider(Agora));

    [Fact]
    public void Generate_RetornaTokenComClaimsDoUsuario()
    {
        var usuario = new Usuario("admin", "hash");

        var token = _generator.Generate(usuario);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token.AccessToken);
        Assert.Equal(usuario.Id.ToString(), jwt.Subject);
        Assert.Equal("LarCooperativa", jwt.Issuer);
        Assert.Equal("LarCooperativa.Api", Assert.Single(jwt.Audiences));
        Assert.Equal("admin", jwt.GetClaim(JwtRegisteredClaimNames.UniqueName).Value);
    }

    [Fact]
    public void Generate_TokenExpiraConformeConfiguracao()
    {
        var token = _generator.Generate(new Usuario("admin", "hash"));

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token.AccessToken);
        Assert.Equal(Agora.AddMinutes(60).UtcDateTime, jwt.ValidTo);
        Assert.Equal(3600, token.ExpiresInSeconds);
        Assert.Equal("Bearer", token.TokenType);
    }
}
