namespace LarCooperativa.Api.Contracts;

/// <summary>Parâmetros de paginação vindos da query string (?pagina=&amp;tamanhoPagina=).</summary>
public sealed record PaginationQuery
{
    public const int TamanhoMaximo = 100;

    public int Pagina { get; init; } = 1;

    public int TamanhoPagina { get; init; } = 20;
}
