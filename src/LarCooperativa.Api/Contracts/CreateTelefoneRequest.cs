using System.ComponentModel.DataAnnotations;
using LarCooperativa.Api.Domain;

namespace LarCooperativa.Api.Contracts;

public sealed record CreateTelefoneRequest
{
    [Required]
    public required TipoTelefone Tipo { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string Numero { get; init; }
}
