using System.Net.Http.Headers;
using System.Net.Http.Json;
using LarCooperativa.Api.Contracts;

namespace LarCooperativa.IntegrationTests;

internal static class AuthClientExtensions
{
    // Credenciais do usuário semeado na inicialização (Auth:AdminUsuario / Auth:AdminSenha)
    public const string AdminUsuario = "admin";
    public const string AdminSenha = "admin123";

    public static async Task<HttpClient> CreateAuthenticatedClientAsync(this WebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient();
        var token = await client.LoginAsync(AdminUsuario, AdminSenha);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task<string> LoginAsync(this HttpClient client, string usuario, string senha)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest { Usuario = usuario, Senha = senha });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TokenResponse>(TestJson.Options);
        return body!.AccessToken;
    }
}
