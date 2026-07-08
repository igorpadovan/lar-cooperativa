using FluentValidation.TestHelper;
using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Validation;

namespace LarCooperativa.UnitTests.Validation;

public class PaginationQueryValidatorTests
{
    private readonly PaginationQueryValidator _validator = new();

    [Fact]
    public void Validar_ComPadroes_NaoTemErros()
    {
        var resultado = _validator.TestValidate(new PaginationQuery());

        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validar_ComPaginaMenorQueUm_TemErroEmPagina(int pagina)
    {
        var query = new PaginationQuery { Pagina = pagina };

        var resultado = _validator.TestValidate(query);

        resultado.ShouldHaveValidationErrorFor(q => q.Pagina);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validar_ComTamanhoForaDoIntervalo_TemErroEmTamanhoPagina(int tamanho)
    {
        var query = new PaginationQuery { TamanhoPagina = tamanho };

        var resultado = _validator.TestValidate(query);

        resultado.ShouldHaveValidationErrorFor(q => q.TamanhoPagina);
    }

    [Fact]
    public void Validar_ComTamanhoNoLimiteMaximo_NaoTemErros()
    {
        var query = new PaginationQuery { TamanhoPagina = 100 };

        var resultado = _validator.TestValidate(query);

        resultado.ShouldNotHaveAnyValidationErrors();
    }
}
