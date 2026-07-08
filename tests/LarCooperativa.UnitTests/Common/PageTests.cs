using LarCooperativa.Api.Common;

namespace LarCooperativa.UnitTests.Common;

public class PageTests
{
    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(1, 20, 1)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    [InlineData(5, 2, 3)]
    public void TotalPaginas_ArredondaParaCima(int totalItens, int tamanhoPagina, int totalPaginasEsperado)
    {
        var page = new Page<string>([], Pagina: 1, tamanhoPagina, totalItens);

        Assert.Equal(totalPaginasEsperado, page.TotalPaginas);
    }
}
