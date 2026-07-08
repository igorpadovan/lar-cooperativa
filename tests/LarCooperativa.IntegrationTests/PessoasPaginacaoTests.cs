using System.Net;
using System.Net.Http.Json;
using LarCooperativa.Api.Contracts;

namespace LarCooperativa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class PessoasPaginacaoTests(ApiFactory factory) : IAsyncLifetime
{
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _client = await factory.CreateAuthenticatedClientAsync();
        for (var i = 0; i < 3; i++)
        {
            await CriarPessoaAsync();
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task CriarPessoaAsync()
    {
        var request = new CreatePessoaRequest
        {
            Nome = "Pessoa da Paginação",
            Cpf = CpfGenerator.Valido(),
            DataNascimento = new DateOnly(1990, 5, 20),
        };
        var response = await _client.PostAsJsonAsync("/api/pessoas", request);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Get_SemParametros_RetornaPrimeiraPaginaComPadroes()
    {
        var response = await _client.GetAsync("/api/pessoas");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var pagina = await response.Content.ReadFromJsonAsync<PagedResponse<PessoaResponse>>();
        Assert.NotNull(pagina);
        Assert.Equal(1, pagina.Pagina);
        Assert.Equal(20, pagina.TamanhoPagina);
        Assert.True(pagina.Itens.Count <= 20);
        Assert.True(pagina.TotalItens >= 3);
    }

    [Fact]
    public async Task Get_ComTamanhoDePagina_LimitaItensECalculaTotais()
    {
        var response = await _client.GetAsync("/api/pessoas?tamanhoPagina=2");

        var pagina = await response.Content.ReadFromJsonAsync<PagedResponse<PessoaResponse>>();
        Assert.NotNull(pagina);
        Assert.Equal(2, pagina.Itens.Count);
        Assert.Equal((int)Math.Ceiling(pagina.TotalItens / 2.0), pagina.TotalPaginas);
    }

    [Fact]
    public async Task Get_PaginasDiferentes_NaoRepetemItens()
    {
        var primeira = await _client.GetFromJsonAsync<PagedResponse<PessoaResponse>>(
            "/api/pessoas?pagina=1&tamanhoPagina=2");
        var segunda = await _client.GetFromJsonAsync<PagedResponse<PessoaResponse>>(
            "/api/pessoas?pagina=2&tamanhoPagina=2");

        Assert.NotNull(primeira);
        Assert.NotNull(segunda);
        var idsDaPrimeira = primeira.Itens.Select(p => p.Id).ToHashSet();
        Assert.DoesNotContain(segunda.Itens, p => idsDaPrimeira.Contains(p.Id));
    }

    [Theory]
    [InlineData("pagina=0")]
    [InlineData("pagina=-1")]
    [InlineData("tamanhoPagina=0")]
    [InlineData("tamanhoPagina=101")]
    public async Task Get_ComPaginacaoInvalida_Retorna400(string queryString)
    {
        var response = await _client.GetAsync($"/api/pessoas?{queryString}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
