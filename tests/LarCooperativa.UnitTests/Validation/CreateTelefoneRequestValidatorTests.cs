using FluentValidation.TestHelper;
using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Domain;
using LarCooperativa.Api.Validation;

namespace LarCooperativa.UnitTests.Validation;

public class CreateTelefoneRequestValidatorTests
{
    private readonly CreateTelefoneRequestValidator _validator = new();

    private static CreateTelefoneRequest RequestValido() => new()
    {
        Tipo = TipoTelefone.Celular,
        Numero = "(11) 91234-5678",
    };

    [Fact]
    public void Validar_ComCelularComMascara_NaoTemErros()
    {
        var resultado = _validator.TestValidate(RequestValido());

        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(TipoTelefone.Residencial)]
    [InlineData(TipoTelefone.Comercial)]
    public void Validar_FixoComDezDigitos_NaoTemErros(TipoTelefone tipo)
    {
        var request = new CreateTelefoneRequest { Tipo = tipo, Numero = "(11) 3333-4444" };

        var resultado = _validator.TestValidate(request);

        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_CelularSemOnzeDigitos_TemErroEmNumero()
    {
        var request = RequestValido() with { Numero = "(11) 1234-5678" };

        var resultado = _validator.TestValidate(request);

        resultado.ShouldHaveValidationErrorFor(r => r.Numero);
    }

    [Theory]
    [InlineData(TipoTelefone.Residencial)]
    [InlineData(TipoTelefone.Comercial)]
    public void Validar_FixoComOnzeDigitos_TemErroEmNumero(TipoTelefone tipo)
    {
        var request = new CreateTelefoneRequest { Tipo = tipo, Numero = "11912345678" };

        var resultado = _validator.TestValidate(request);

        resultado.ShouldHaveValidationErrorFor(r => r.Numero);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("11abcd5678")]
    [InlineData("+55 11 91234-5678")]
    public void Validar_ComNumeroMalFormado_TemErroEmNumero(string numero)
    {
        var request = RequestValido() with { Numero = numero };

        var resultado = _validator.TestValidate(request);

        resultado.ShouldHaveValidationErrorFor(r => r.Numero);
    }

    [Fact]
    public void Validar_ComTipoForaDoEnum_TemErroEmTipo()
    {
        var request = RequestValido() with { Tipo = (TipoTelefone)99 };

        var resultado = _validator.TestValidate(request);

        resultado.ShouldHaveValidationErrorFor(r => r.Tipo);
    }
}
