using LarCooperativa.Api.Auth;
using LarCooperativa.Api.Common;
using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Domain;
using Microsoft.AspNetCore.Identity;

namespace LarCooperativa.Api.Services;

public sealed class AuthService(
    IUsuarioRepository repository,
    IPasswordHasher<Usuario> passwordHasher,
    ITokenGenerator tokenGenerator) : IAuthService
{
    public async Task<Result<TokenResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var usuario = await repository.GetByNomeUsuarioAsync(request.Usuario, cancellationToken);
        if (usuario is null)
        {
            return Result<TokenResponse>.Failure(AuthErrors.CredenciaisInvalidas);
        }

        var verificacao = passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, request.Senha);
        if (verificacao == PasswordVerificationResult.Failed)
        {
            return Result<TokenResponse>.Failure(AuthErrors.CredenciaisInvalidas);
        }

        return Result<TokenResponse>.Success(tokenGenerator.Generate(usuario));
    }
}
