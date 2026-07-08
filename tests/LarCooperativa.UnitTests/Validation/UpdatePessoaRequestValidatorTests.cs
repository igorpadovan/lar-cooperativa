using FluentValidation.TestHelper;
using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Validation;
using Microsoft.Extensions.Time.Testing;

namespace LarCooperativa.UnitTests.Validation;

public class UpdatePessoaRequestValidatorTests
{
    private static readonly DateTimeOffset Agora = new(2026, 7, 7, 12, 0, 0, TimeSpan.Zero);

    private readonly UpdatePessoaRequestValidator _validator = new(new FakeTimeProvider(Agora));

    private static UpdatePessoaRequest RequestValido() => new()
    {
        Nome = "Maria da Silva",
        Cpf = "529.982.247-25",
        DataNascimento = new DateOnly(1990, 5, 20),
        EstaAtivo = true,
    };

    [Fact]
    public void Validar_ComDadosValidos_NaoTemErros()
    {
        var resultado = _validator.TestValidate(RequestValido());

        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_ComNomeVazio_TemErroEmNome()
    {
        var request = RequestValido() with { Nome = "" };

        var resultado = _validator.TestValidate(request);

        resultado.ShouldHaveValidationErrorFor(r => r.Nome);
    }

    [Fact]
    public void Validar_ComCpfInvalido_TemErroEmCpf()
    {
        var request = RequestValido() with { Cpf = "123" };

        var resultado = _validator.TestValidate(request);

        resultado.ShouldHaveValidationErrorFor(r => r.Cpf);
    }

    [Fact]
    public void Validar_ComDataNascimentoFutura_TemErroEmDataNascimento()
    {
        var request = RequestValido() with { DataNascimento = new DateOnly(2026, 7, 8) };

        var resultado = _validator.TestValidate(request);

        resultado.ShouldHaveValidationErrorFor(r => r.DataNascimento);
    }
}
