using FluentValidation.TestHelper;
using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Validation;
using Microsoft.Extensions.Time.Testing;

namespace LarCooperativa.UnitTests.Validation;

public class CreatePessoaRequestValidatorTests
{
    private static readonly DateTimeOffset Agora = new(2026, 7, 7, 12, 0, 0, TimeSpan.Zero);

    private readonly CreatePessoaRequestValidator _validator = new(new FakeTimeProvider(Agora));

    private static CreatePessoaRequest RequestValido() => new()
    {
        Nome = "Maria da Silva",
        Cpf = "529.982.247-25",
        DataNascimento = new DateOnly(1990, 5, 20),
    };

    [Fact]
    public void Validar_ComDadosValidos_NaoTemErros()
    {
        var resultado = _validator.TestValidate(RequestValido());

        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_ComNomeVazio_TemErroEmNome(string nome)
    {
        var request = RequestValido() with { Nome = nome };

        var resultado = _validator.TestValidate(request);

        resultado.ShouldHaveValidationErrorFor(r => r.Nome);
    }

    [Fact]
    public void Validar_ComNomeAcimaDoLimite_TemErroEmNome()
    {
        var request = RequestValido() with { Nome = new string('a', 201) };

        var resultado = _validator.TestValidate(request);

        resultado.ShouldHaveValidationErrorFor(r => r.Nome);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("111.111.111-11")] // todos os dígitos iguais
    [InlineData("529.982.247-24")] // dígito verificador errado
    [InlineData("5299822472a")]
    public void Validar_ComCpfInvalido_TemErroEmCpf(string cpf)
    {
        var request = RequestValido() with { Cpf = cpf };

        var resultado = _validator.TestValidate(request);

        resultado.ShouldHaveValidationErrorFor(r => r.Cpf);
    }

    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    public void Validar_ComCpfValidoComOuSemMascara_NaoTemErroEmCpf(string cpf)
    {
        var request = RequestValido() with { Cpf = cpf };

        var resultado = _validator.TestValidate(request);

        resultado.ShouldNotHaveValidationErrorFor(r => r.Cpf);
    }

    [Fact]
    public void Validar_ComDataNascimentoFutura_TemErroEmDataNascimento()
    {
        var request = RequestValido() with { DataNascimento = new DateOnly(2026, 7, 8) };

        var resultado = _validator.TestValidate(request);

        resultado.ShouldHaveValidationErrorFor(r => r.DataNascimento);
    }

    [Fact]
    public void Validar_ComDataNascimentoHoje_NaoTemErroEmDataNascimento()
    {
        var request = RequestValido() with { DataNascimento = new DateOnly(2026, 7, 7) };

        var resultado = _validator.TestValidate(request);

        resultado.ShouldNotHaveValidationErrorFor(r => r.DataNascimento);
    }
}
