using LarCooperativa.Api.Common;

namespace LarCooperativa.Api.Contracts;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas)
{
    public static PagedResponse<T> FromPage<TDomain>(Page<TDomain> page, Func<TDomain, T> map) => new(
        page.Itens.Select(map).ToList(),
        page.Pagina,
        page.TamanhoPagina,
        page.TotalItens,
        page.TotalPaginas);
}
