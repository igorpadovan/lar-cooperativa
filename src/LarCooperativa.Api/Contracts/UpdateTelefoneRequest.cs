using LarCooperativa.Api.Domain;

namespace LarCooperativa.Api.Contracts;

public sealed record UpdateTelefoneRequest
{
    public required TipoTelefone Tipo { get; init; }

    public required string Numero { get; init; }
}
