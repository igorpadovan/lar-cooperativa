using System.Net;
using System.Net.Http.Json;
using LarCooperativa.Api.Contracts;

namespace LarCooperativa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class AuthEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_ComCredenciaisValidas_RetornaTokenBearer()
    {
        var request = new LoginRequest
        {
            Usuario = AuthClientExtensions.AdminUsuario,
            Senha = AuthClientExtensions.AdminSenha,
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(TestJson.Options);
        Assert.NotNull(token);
        Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));
        Assert.Equal("Bearer", token.TokenType);
    }

    [Fact]
    public async Task Login_ComSenhaIncorreta_Retorna401()
    {
        var request = new LoginRequest
        {
            Usuario = AuthClientExtensions.AdminUsuario,
            Senha = "senha-errada",
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComUsuarioInexistente_Retorna401()
    {
        var request = new LoginRequest { Usuario = "nao-existe", Senha = "qualquer" };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Endpoints_SemToken_Retornam401()
    {
        var response = await _client.GetAsync("/api/pessoas");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Endpoints_ComTokenInvalido_Retornam401()
    {
        _client.DefaultRequestHeaders.Authorization = new("Bearer", "token-invalido");

        var response = await _client.GetAsync("/api/pessoas");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Endpoints_ComTokenValido_Respondem()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/pessoas");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
