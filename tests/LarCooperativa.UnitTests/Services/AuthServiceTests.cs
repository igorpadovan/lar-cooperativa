using LarCooperativa.Api.Auth;
using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Domain;
using LarCooperativa.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LarCooperativa.UnitTests.Services;

public class AuthServiceTests
{
    private readonly IUsuarioRepository _repository = Substitute.For<IUsuarioRepository>();
    private readonly IPasswordHasher<Usuario> _passwordHasher = Substitute.For<IPasswordHasher<Usuario>>();
    private readonly ITokenGenerator _tokenGenerator = Substitute.For<ITokenGenerator>();
    private readonly AuthService _service;

    private readonly Usuario _usuario = new("admin", "hash-da-senha");

    public AuthServiceTests()
    {
        _service = new AuthService(
            _repository, _passwordHasher, _tokenGenerator, NullLogger<AuthService>.Instance);
    }

    private static LoginRequest Request(string senha = "admin123") => new()
    {
        Usuario = "admin",
        Senha = senha,
    };

    private void DadoQueUsuarioExiste() =>
        _repository.GetByNomeUsuarioAsync("admin", Arg.Any<CancellationToken>()).Returns(_usuario);

    [Fact]
    public async Task LoginAsync_ComCredenciaisValidas_RetornaToken()
    {
        DadoQueUsuarioExiste();
        _passwordHasher.VerifyHashedPassword(_usuario, "hash-da-senha", "admin123")
            .Returns(PasswordVerificationResult.Success);
        var token = new TokenResponse("token-jwt", "Bearer", 3600);
        _tokenGenerator.Generate(_usuario).Returns(token);

        var result = await _service.LoginAsync(Request(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(token, result.Value);
    }

    [Fact]
    public async Task LoginAsync_ComSenhaIncorreta_RetornaCredenciaisInvalidas()
    {
        DadoQueUsuarioExiste();
        _passwordHasher.VerifyHashedPassword(_usuario, "hash-da-senha", "senha-errada")
            .Returns(PasswordVerificationResult.Failed);

        var result = await _service.LoginAsync(Request("senha-errada"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.CredenciaisInvalidas, result.Error);
    }

    [Fact]
    public async Task LoginAsync_ComUsuarioInexistente_RetornaCredenciaisInvalidas()
    {
        var result = await _service.LoginAsync(Request(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.CredenciaisInvalidas, result.Error);
        _tokenGenerator.DidNotReceive().Generate(Arg.Any<Usuario>());
    }

    [Fact]
    public async Task LoginAsync_ComHashDesatualizado_AindaAceitaCredenciais()
    {
        DadoQueUsuarioExiste();
        _passwordHasher.VerifyHashedPassword(_usuario, "hash-da-senha", "admin123")
            .Returns(PasswordVerificationResult.SuccessRehashNeeded);
        _tokenGenerator.Generate(_usuario).Returns(new TokenResponse("token-jwt", "Bearer", 3600));

        var result = await _service.LoginAsync(Request(), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
