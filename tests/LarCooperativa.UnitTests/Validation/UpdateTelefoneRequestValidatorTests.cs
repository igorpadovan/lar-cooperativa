using FluentValidation.TestHelper;
using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Domain;
using LarCooperativa.Api.Validation;

namespace LarCooperativa.UnitTests.Validation;

public class UpdateTelefoneRequestValidatorTests
{
    private readonly UpdateTelefoneRequestValidator _validator = new();

    private static UpdateTelefoneRequest RequestValido() => new()
    {
        Tipo = TipoTelefone.Residencial,
        Numero = "(11) 3333-4444",
    };

    [Fact]
    public void Validar_ComDadosValidos_NaoTemErros()
    {
        var resultado = _validator.TestValidate(RequestValido());

        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_ComNumeroIncompativelComTipo_TemErroEmNumero()
    {
        var request = RequestValido() with { Numero = "11912345678" };

        var resultado = _validator.TestValidate(request);

        resultado.ShouldHaveValidationErrorFor(r => r.Numero);
    }
}
