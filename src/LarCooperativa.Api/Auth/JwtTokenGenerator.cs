using System.Text;
using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LarCooperativa.Api.Auth;

public sealed class JwtTokenGenerator(IOptions<JwtSettings> options, TimeProvider timeProvider) : ITokenGenerator
{
    public TokenResponse Generate(Usuario usuario)
    {
        var settings = options.Value;
        var agora = timeProvider.GetUtcNow();
        var expiraEm = agora.AddMinutes(settings.ExpirationMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            IssuedAt = agora.UtcDateTime,
            NotBefore = agora.UtcDateTime,
            Expires = expiraEm.UtcDateTime,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = usuario.Id.ToString(),
                [JwtRegisteredClaimNames.UniqueName] = usuario.NomeUsuario,
            },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
                SecurityAlgorithms.HmacSha256),
        };

        var accessToken = new JsonWebTokenHandler().CreateToken(descriptor);

        return new TokenResponse(accessToken, "Bearer", settings.ExpirationMinutes * 60);
    }
}
