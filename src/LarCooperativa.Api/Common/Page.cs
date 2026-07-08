namespace LarCooperativa.Api.Common;

/// <summary>Uma página de resultados com os totais para navegação.</summary>
public sealed record Page<T>(IReadOnlyList<T> Itens, int Pagina, int TamanhoPagina, int TotalItens)
{
    public int TotalPaginas => (int)Math.Ceiling((double)TotalItens / TamanhoPagina);
}
