using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Domain;
using LarCooperativa.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LarCooperativa.UnitTests.Services;

public class TelefoneServiceTests
{
    private readonly ITelefoneRepository _telefoneRepository = Substitute.For<ITelefoneRepository>();
    private readonly IPessoaRepository _pessoaRepository = Substitute.For<IPessoaRepository>();
    private readonly TelefoneService _service;

    private readonly Pessoa _pessoa =
        new("Maria da Silva", Cpf.TryCreate("52998224725")!, new DateOnly(1990, 5, 20));

    public TelefoneServiceTests()
    {
        _service = new TelefoneService(
            _telefoneRepository, _pessoaRepository, NullLogger<TelefoneService>.Instance);
    }

    private void DadoQuePessoaExiste() =>
        _pessoaRepository.GetByIdAsync(_pessoa.Id, Arg.Any<CancellationToken>()).Returns(_pessoa);

    private static CreateTelefoneRequest CreateRequestValido() => new()
    {
        Tipo = TipoTelefone.Celular,
        Numero = "(11) 91234-5678",
    };

    private Telefone TelefoneExistente() =>
        new(_pessoa.Id, TipoTelefone.Residencial, "1133334444");

    [Fact]
    public async Task CreateAsync_ComDadosValidos_CriaTelefoneComNumeroNormalizado()
    {
        DadoQuePessoaExiste();

        var result = await _service.CreateAsync(_pessoa.Id, CreateRequestValido(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(_pessoa.Id, result.Value.PessoaId);
        Assert.Equal(TipoTelefone.Celular, result.Value.Tipo);
        Assert.Equal("11912345678", result.Value.Numero);
        await _telefoneRepository.Received(1).AddAsync(result.Value, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_QuandoPessoaNaoExiste_RetornaPessoaNaoEncontrada()
    {
        var result = await _service.CreateAsync(Guid.NewGuid(), CreateRequestValido(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PessoaErrors.NaoEncontrada, result.Error);
        await _telefoneRepository.DidNotReceive().AddAsync(Arg.Any<Telefone>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ComNumeroInvalido_RetornaNumeroInvalidoSemPersistir()
    {
        DadoQuePessoaExiste();
        var request = CreateRequestValido() with { Numero = "(11) 1234-5678" }; // 10 dígitos para celular

        var result = await _service.CreateAsync(_pessoa.Id, request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(TelefoneErrors.NumeroInvalido, result.Error);
        await _telefoneRepository.DidNotReceive().AddAsync(Arg.Any<Telefone>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ComNumeroDuplicadoNaMesmaPessoa_RetornaConflito()
    {
        DadoQuePessoaExiste();
        _telefoneRepository.ExistsByNumeroAsync(_pessoa.Id, "11912345678", null, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _service.CreateAsync(_pessoa.Id, CreateRequestValido(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(TelefoneErrors.NumeroDuplicado, result.Error);
        await _telefoneRepository.DidNotReceive().AddAsync(Arg.Any<Telefone>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllAsync_QuandoPessoaNaoExiste_RetornaPessoaNaoEncontrada()
    {
        var result = await _service.GetAllAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PessoaErrors.NaoEncontrada, result.Error);
    }

    [Fact]
    public async Task GetAllAsync_QuandoPessoaExiste_RetornaTelefones()
    {
        DadoQuePessoaExiste();
        var telefone = TelefoneExistente();
        _telefoneRepository.GetAllByPessoaAsync(_pessoa.Id, Arg.Any<CancellationToken>())
            .Returns([telefone]);

        var result = await _service.GetAllAsync(_pessoa.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value, telefone);
    }

    [Fact]
    public async Task UpdateAsync_QuandoTelefoneNaoExiste_RetornaNaoEncontrado()
    {
        var request = new UpdateTelefoneRequest { Tipo = TipoTelefone.Celular, Numero = "11912345678" };

        var result = await _service.UpdateAsync(_pessoa.Id, Guid.NewGuid(), request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(TelefoneErrors.NaoEncontrado, result.Error);
    }

    [Fact]
    public async Task UpdateAsync_ComDadosValidos_AtualizaCamposEPersiste()
    {
        var telefone = TelefoneExistente();
        _telefoneRepository.GetByIdAsync(_pessoa.Id, telefone.Id, Arg.Any<CancellationToken>())
            .Returns(telefone);
        var request = new UpdateTelefoneRequest { Tipo = TipoTelefone.Celular, Numero = "(11) 98888-7777" };

        var result = await _service.UpdateAsync(_pessoa.Id, telefone.Id, request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TipoTelefone.Celular, telefone.Tipo);
        Assert.Equal("11988887777", telefone.Numero);
        await _telefoneRepository.Received(1).UpdateAsync(telefone, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ComNumeroDeOutroTelefoneDaPessoa_RetornaConflito()
    {
        var telefone = TelefoneExistente();
        _telefoneRepository.GetByIdAsync(_pessoa.Id, telefone.Id, Arg.Any<CancellationToken>())
            .Returns(telefone);
        _telefoneRepository.ExistsByNumeroAsync(_pessoa.Id, "11912345678", telefone.Id, Arg.Any<CancellationToken>())
            .Returns(true);
        var request = new UpdateTelefoneRequest { Tipo = TipoTelefone.Celular, Numero = "11912345678" };

        var result = await _service.UpdateAsync(_pessoa.Id, telefone.Id, request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(TelefoneErrors.NumeroDuplicado, result.Error);
        await _telefoneRepository.DidNotReceive().UpdateAsync(Arg.Any<Telefone>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_QuandoTelefoneNaoExiste_RetornaNaoEncontrado()
    {
        var result = await _service.DeleteAsync(_pessoa.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(TelefoneErrors.NaoEncontrado, result.Error);
    }

    [Fact]
    public async Task DeleteAsync_QuandoTelefoneExiste_RemoveERetornaSucesso()
    {
        var telefone = TelefoneExistente();
        _telefoneRepository.GetByIdAsync(_pessoa.Id, telefone.Id, Arg.Any<CancellationToken>())
            .Returns(telefone);

        var result = await _service.DeleteAsync(_pessoa.Id, telefone.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _telefoneRepository.Received(1).DeleteAsync(telefone, Arg.Any<CancellationToken>());
    }
}
