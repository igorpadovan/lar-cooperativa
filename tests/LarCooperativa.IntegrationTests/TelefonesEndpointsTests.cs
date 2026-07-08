using System.Net;
using System.Net.Http.Json;
using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Domain;

namespace LarCooperativa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class TelefonesEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static CreateTelefoneRequest RequestValido() => new()
    {
        Tipo = TipoTelefone.Celular,
        Numero = "(11) 91234-5678",
    };

    private async Task<PessoaResponse> CriarPessoaAsync()
    {
        var request = new CreatePessoaRequest
        {
            Nome = "Maria da Silva",
            Cpf = CpfGenerator.Valido(),
            DataNascimento = new DateOnly(1990, 5, 20),
        };
        var response = await _client.PostAsJsonAsync("/api/pessoas", request, TestJson.Options);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PessoaResponse>(TestJson.Options))!;
    }

    private async Task<TelefoneResponse> CriarTelefoneAsync(Guid pessoaId, CreateTelefoneRequest? request = null)
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/pessoas/{pessoaId}/telefones", request ?? RequestValido(), TestJson.Options);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TelefoneResponse>(TestJson.Options))!;
    }

    [Fact]
    public async Task Post_ComDadosValidos_Retorna201ComLocationENumeroNormalizado()
    {
        var pessoa = await CriarPessoaAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/pessoas/{pessoa.Id}/telefones", RequestValido(), TestJson.Options);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var telefone = await response.Content.ReadFromJsonAsync<TelefoneResponse>(TestJson.Options);
        Assert.NotNull(telefone);
        Assert.Equal(pessoa.Id, telefone.PessoaId);
        Assert.Equal(TipoTelefone.Celular, telefone.Tipo);
        Assert.Equal("11912345678", telefone.Numero);
        Assert.EndsWith(
            $"/api/pessoas/{pessoa.Id}/telefones/{telefone.Id}",
            response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Post_QuandoPessoaNaoExiste_Retorna404()
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/pessoas/{Guid.NewGuid()}/telefones", RequestValido(), TestJson.Options);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComTipoDesconhecido_Retorna400()
    {
        var pessoa = await CriarPessoaAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/pessoas/{pessoa.Id}/telefones", new { tipo = "Fax", numero = "11912345678" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComNumeroIncompativelComOTipo_Retorna400()
    {
        var pessoa = await CriarPessoaAsync();
        var request = RequestValido() with { Numero = "(11) 3333-4444" }; // 10 dígitos para celular

        var response = await _client.PostAsJsonAsync(
            $"/api/pessoas/{pessoa.Id}/telefones", request, TestJson.Options);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComNumeroDuplicadoNaMesmaPessoa_Retorna409()
    {
        var pessoa = await CriarPessoaAsync();
        await CriarTelefoneAsync(pessoa.Id);

        var response = await _client.PostAsJsonAsync(
            $"/api/pessoas/{pessoa.Id}/telefones",
            RequestValido() with { Numero = "11912345678" },
            TestJson.Options);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComMesmoNumeroParaOutraPessoa_Retorna201()
    {
        var primeira = await CriarPessoaAsync();
        var segunda = await CriarPessoaAsync();
        await CriarTelefoneAsync(primeira.Id);

        var response = await _client.PostAsJsonAsync(
            $"/api/pessoas/{segunda.Id}/telefones", RequestValido(), TestJson.Options);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Get_RetornaTelefonesDaPessoa()
    {
        var pessoa = await CriarPessoaAsync();
        var celular = await CriarTelefoneAsync(pessoa.Id);
        var comercial = await CriarTelefoneAsync(
            pessoa.Id, new CreateTelefoneRequest { Tipo = TipoTelefone.Comercial, Numero = "(11) 3333-4444" });

        var response = await _client.GetAsync($"/api/pessoas/{pessoa.Id}/telefones");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var telefones = await response.Content.ReadFromJsonAsync<List<TelefoneResponse>>(TestJson.Options);
        Assert.NotNull(telefones);
        Assert.Equal(2, telefones.Count);
        Assert.Contains(celular, telefones);
        Assert.Contains(comercial, telefones);
    }

    [Fact]
    public async Task Get_QuandoPessoaNaoExiste_Retorna404()
    {
        var response = await _client.GetAsync($"/api/pessoas/{Guid.NewGuid()}/telefones");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_QuandoExiste_RetornaTelefone()
    {
        var pessoa = await CriarPessoaAsync();
        var telefone = await CriarTelefoneAsync(pessoa.Id);

        var response = await _client.GetAsync($"/api/pessoas/{pessoa.Id}/telefones/{telefone.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var encontrado = await response.Content.ReadFromJsonAsync<TelefoneResponse>(TestJson.Options);
        Assert.Equal(telefone, encontrado);
    }

    [Fact]
    public async Task GetById_QuandoNaoExiste_Retorna404()
    {
        var pessoa = await CriarPessoaAsync();

        var response = await _client.GetAsync($"/api/pessoas/{pessoa.Id}/telefones/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ComDadosValidos_AtualizaERetorna200()
    {
        var pessoa = await CriarPessoaAsync();
        var telefone = await CriarTelefoneAsync(pessoa.Id);
        var request = new UpdateTelefoneRequest { Tipo = TipoTelefone.Residencial, Numero = "(11) 3333-4444" };

        var response = await _client.PutAsJsonAsync(
            $"/api/pessoas/{pessoa.Id}/telefones/{telefone.Id}", request, TestJson.Options);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var atualizado = await response.Content.ReadFromJsonAsync<TelefoneResponse>(TestJson.Options);
        Assert.NotNull(atualizado);
        Assert.Equal(telefone.Id, atualizado.Id);
        Assert.Equal(TipoTelefone.Residencial, atualizado.Tipo);
        Assert.Equal("1133334444", atualizado.Numero);
    }

    [Fact]
    public async Task Put_QuandoNaoExiste_Retorna404()
    {
        var pessoa = await CriarPessoaAsync();
        var request = new UpdateTelefoneRequest { Tipo = TipoTelefone.Celular, Numero = "11912345678" };

        var response = await _client.PutAsJsonAsync(
            $"/api/pessoas/{pessoa.Id}/telefones/{Guid.NewGuid()}", request, TestJson.Options);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ComNumeroDeOutroTelefoneDaPessoa_Retorna409()
    {
        var pessoa = await CriarPessoaAsync();
        await CriarTelefoneAsync(pessoa.Id); // 11912345678
        var alvo = await CriarTelefoneAsync(
            pessoa.Id, new CreateTelefoneRequest { Tipo = TipoTelefone.Comercial, Numero = "(11) 3333-4444" });
        var request = new UpdateTelefoneRequest { Tipo = TipoTelefone.Celular, Numero = "11912345678" };

        var response = await _client.PutAsJsonAsync(
            $"/api/pessoas/{pessoa.Id}/telefones/{alvo.Id}", request, TestJson.Options);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_QuandoExiste_Retorna204ETelefoneDeixaDeExistir()
    {
        var pessoa = await CriarPessoaAsync();
        var telefone = await CriarTelefoneAsync(pessoa.Id);

        var response = await _client.DeleteAsync($"/api/pessoas/{pessoa.Id}/telefones/{telefone.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await _client.GetAsync($"/api/pessoas/{pessoa.Id}/telefones/{telefone.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_QuandoNaoExiste_Retorna404()
    {
        var pessoa = await CriarPessoaAsync();

        var response = await _client.DeleteAsync($"/api/pessoas/{pessoa.Id}/telefones/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeletePessoa_ComTelefones_RemoveEmCascata()
    {
        var pessoa = await CriarPessoaAsync();
        await CriarTelefoneAsync(pessoa.Id);

        var response = await _client.DeleteAsync($"/api/pessoas/{pessoa.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
