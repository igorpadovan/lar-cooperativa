using LarCooperativa.Api.Auth;
using LarCooperativa.Api.Common;
using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Domain;
using Microsoft.AspNetCore.Identity;

namespace LarCooperativa.Api.Services;

public sealed class AuthService(
    IUsuarioRepository repository,
    IPasswordHasher<Usuario> passwordHasher,
    ITokenGenerator tokenGenerator,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<Result<TokenResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var usuario = await repository.GetByNomeUsuarioAsync(request.Usuario, cancellationToken);
        if (usuario is null)
        {
            logger.LogWarning("Falha de autenticação para o usuário {NomeUsuario}", request.Usuario);
            return Result<TokenResponse>.Failure(AuthErrors.CredenciaisInvalidas);
        }

        var verificacao = passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, request.Senha);
        if (verificacao == PasswordVerificationResult.Failed)
        {
            logger.LogWarning("Falha de autenticação para o usuário {NomeUsuario}", request.Usuario);
            return Result<TokenResponse>.Failure(AuthErrors.CredenciaisInvalidas);
        }

        logger.LogInformation("Usuário {NomeUsuario} autenticado", usuario.NomeUsuario);
        return Result<TokenResponse>.Success(tokenGenerator.Generate(usuario));
    }
}
