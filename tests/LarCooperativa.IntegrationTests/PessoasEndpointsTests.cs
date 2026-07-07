using System.Net;
using System.Net.Http.Json;
using LarCooperativa.Api.Contracts;

namespace LarCooperativa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class PessoasEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static CreatePessoaRequest RequestValido(string? cpf = null) => new()
    {
        Nome = "Maria da Silva",
        Cpf = cpf ?? CpfGenerator.Valido(),
        DataNascimento = new DateOnly(1990, 5, 20),
    };

    private async Task<PessoaResponse> CriarPessoaAsync(CreatePessoaRequest? request = null)
    {
        var response = await _client.PostAsJsonAsync("/api/pessoas", request ?? RequestValido());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PessoaResponse>())!;
    }

    [Fact]
    public async Task Post_ComDadosValidos_Retorna201ComLocationEPessoaAtiva()
    {
        var request = RequestValido();

        var response = await _client.PostAsJsonAsync("/api/pessoas", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var pessoa = await response.Content.ReadFromJsonAsync<PessoaResponse>();
        Assert.NotNull(pessoa);
        Assert.NotEqual(Guid.Empty, pessoa.Id);
        Assert.Equal(request.Nome, pessoa.Nome);
        Assert.Equal(request.Cpf, pessoa.Cpf);
        Assert.True(pessoa.EstaAtivo);
        Assert.EndsWith($"/api/pessoas/{pessoa.Id}", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Post_ComCpfComMascara_PersisteCpfNormalizado()
    {
        var cpf = CpfGenerator.Valido();
        var cpfComMascara = $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}";

        var pessoa = await CriarPessoaAsync(RequestValido(cpfComMascara));

        Assert.Equal(cpf, pessoa.Cpf);
    }

    [Fact]
    public async Task Post_ComCpfInvalido_Retorna400()
    {
        var response = await _client.PostAsJsonAsync("/api/pessoas", RequestValido(cpf: "123"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComNomeVazio_Retorna400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/pessoas",
            new { nome = "", cpf = CpfGenerator.Valido(), dataNascimento = "1990-05-20" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComDataNascimentoFutura_Retorna400()
    {
        var request = RequestValido() with { DataNascimento = new DateOnly(2999, 1, 1) };

        var response = await _client.PostAsJsonAsync("/api/pessoas", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComCpfDuplicado_Retorna409()
    {
        var request = RequestValido();
        await CriarPessoaAsync(request);

        var response = await _client.PostAsJsonAsync("/api/pessoas", request with { Nome = "Outro Nome" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Get_RetornaPessoasCriadas()
    {
        var pessoa = await CriarPessoaAsync();

        var response = await _client.GetAsync("/api/pessoas");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var pessoas = await response.Content.ReadFromJsonAsync<List<PessoaResponse>>();
        Assert.NotNull(pessoas);
        Assert.Contains(pessoas, p => p.Id == pessoa.Id);
    }

    [Fact]
    public async Task GetById_QuandoExiste_RetornaPessoa()
    {
        var pessoa = await CriarPessoaAsync();

        var response = await _client.GetAsync($"/api/pessoas/{pessoa.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var encontrada = await response.Content.ReadFromJsonAsync<PessoaResponse>();
        Assert.Equal(pessoa, encontrada);
    }

    [Fact]
    public async Task GetById_QuandoNaoExiste_Retorna404()
    {
        var response = await _client.GetAsync($"/api/pessoas/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ComDadosValidos_AtualizaERetorna200()
    {
        var pessoa = await CriarPessoaAsync();
        var novoCpf = CpfGenerator.Valido();
        var request = new UpdatePessoaRequest
        {
            Nome = "Maria Atualizada",
            Cpf = novoCpf,
            DataNascimento = new DateOnly(1991, 6, 21),
            EstaAtivo = false,
        };

        var response = await _client.PutAsJsonAsync($"/api/pessoas/{pessoa.Id}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var atualizada = await response.Content.ReadFromJsonAsync<PessoaResponse>();
        Assert.NotNull(atualizada);
        Assert.Equal(pessoa.Id, atualizada.Id);
        Assert.Equal("Maria Atualizada", atualizada.Nome);
        Assert.Equal(novoCpf, atualizada.Cpf);
        Assert.Equal(new DateOnly(1991, 6, 21), atualizada.DataNascimento);
        Assert.False(atualizada.EstaAtivo);
    }

    [Fact]
    public async Task Put_QuandoNaoExiste_Retorna404()
    {
        var request = new UpdatePessoaRequest
        {
            Nome = "Qualquer Nome",
            Cpf = CpfGenerator.Valido(),
            DataNascimento = new DateOnly(1990, 5, 20),
            EstaAtivo = true,
        };

        var response = await _client.PutAsJsonAsync($"/api/pessoas/{Guid.NewGuid()}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ComCpfDeOutraPessoa_Retorna409()
    {
        var existente = await CriarPessoaAsync();
        var alvo = await CriarPessoaAsync();
        var request = new UpdatePessoaRequest
        {
            Nome = alvo.Nome,
            Cpf = existente.Cpf,
            DataNascimento = alvo.DataNascimento,
            EstaAtivo = true,
        };

        var response = await _client.PutAsJsonAsync($"/api/pessoas/{alvo.Id}", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_QuandoExiste_Retorna204EPessoaDeixaDeExistir()
    {
        var pessoa = await CriarPessoaAsync();

        var response = await _client.DeleteAsync($"/api/pessoas/{pessoa.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await _client.GetAsync($"/api/pessoas/{pessoa.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_QuandoNaoExiste_Retorna404()
    {
        var response = await _client.DeleteAsync($"/api/pessoas/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
