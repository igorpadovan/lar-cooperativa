using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Domain;
using LarCooperativa.Api.Services;
using NSubstitute;

namespace LarCooperativa.UnitTests.Services;

public class PessoaServiceTests
{
    private readonly IPessoaRepository _repository = Substitute.For<IPessoaRepository>();
    private readonly PessoaService _service;

    public PessoaServiceTests()
    {
        _service = new PessoaService(_repository);
    }

    private static CreatePessoaRequest CreateRequestValido() => new()
    {
        Nome = "Maria da Silva",
        Cpf = "529.982.247-25",
        DataNascimento = new DateOnly(1990, 5, 20),
    };

    private static UpdatePessoaRequest UpdateRequestValido() => new()
    {
        Nome = "Maria da Silva Santos",
        Cpf = "111.444.777-35",
        DataNascimento = new DateOnly(1990, 5, 21),
        EstaAtivo = false,
    };

    private static Pessoa PessoaExistente() =>
        new("João Pereira", Cpf.TryCreate("52998224725")!, new DateOnly(1985, 1, 10));

    [Fact]
    public async Task CreateAsync_ComDadosValidos_CriaPessoaAtivaComCpfNormalizado()
    {
        var result = await _service.CreateAsync(CreateRequestValido(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Maria da Silva", result.Value.Nome);
        Assert.Equal("52998224725", result.Value.Cpf.Valor);
        Assert.Equal(new DateOnly(1990, 5, 20), result.Value.DataNascimento);
        Assert.True(result.Value.EstaAtivo);
        await _repository.Received(1).AddAsync(result.Value, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ComCpfInvalido_RetornaCpfInvalidoSemPersistir()
    {
        var request = CreateRequestValido() with { Cpf = "123" };

        var result = await _service.CreateAsync(request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PessoaErrors.CpfInvalido, result.Error);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Pessoa>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ComCpfDuplicado_RetornaConflito()
    {
        _repository.ExistsByCpfAsync(Arg.Any<Cpf>(), null, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _service.CreateAsync(CreateRequestValido(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PessoaErrors.CpfDuplicado, result.Error);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Pessoa>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_QuandoPessoaNaoExiste_RetornaNaoEncontrada()
    {
        var result = await _service.UpdateAsync(Guid.NewGuid(), UpdateRequestValido(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PessoaErrors.NaoEncontrada, result.Error);
    }

    [Fact]
    public async Task UpdateAsync_ComDadosValidos_AtualizaCamposEPersiste()
    {
        var pessoa = PessoaExistente();
        _repository.GetByIdAsync(pessoa.Id, Arg.Any<CancellationToken>()).Returns(pessoa);

        var result = await _service.UpdateAsync(pessoa.Id, UpdateRequestValido(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Maria da Silva Santos", pessoa.Nome);
        Assert.Equal("11144477735", pessoa.Cpf.Valor);
        Assert.Equal(new DateOnly(1990, 5, 21), pessoa.DataNascimento);
        Assert.False(pessoa.EstaAtivo);
        await _repository.Received(1).UpdateAsync(pessoa, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ComCpfDeOutraPessoa_RetornaConflito()
    {
        var pessoa = PessoaExistente();
        _repository.GetByIdAsync(pessoa.Id, Arg.Any<CancellationToken>()).Returns(pessoa);
        _repository.ExistsByCpfAsync(Arg.Any<Cpf>(), pessoa.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _service.UpdateAsync(pessoa.Id, UpdateRequestValido(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PessoaErrors.CpfDuplicado, result.Error);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Pessoa>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_QuandoPessoaNaoExiste_RetornaNaoEncontrada()
    {
        var result = await _service.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PessoaErrors.NaoEncontrada, result.Error);
    }

    [Fact]
    public async Task DeleteAsync_QuandoPessoaExiste_RemoveERetornaSucesso()
    {
        var pessoa = PessoaExistente();
        _repository.GetByIdAsync(pessoa.Id, Arg.Any<CancellationToken>()).Returns(pessoa);

        var result = await _service.DeleteAsync(pessoa.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _repository.Received(1).DeleteAsync(pessoa, Arg.Any<CancellationToken>());
    }
}
