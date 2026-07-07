using System.ComponentModel.DataAnnotations;

namespace LarCooperativa.Api.Contracts;

public sealed record UpdatePessoaRequest
{
    [Required(AllowEmptyStrings = false)]
    [MaxLength(200)]
    public required string Nome { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string Cpf { get; init; }

    [Required]
    public required DateOnly DataNascimento { get; init; }

    [Required]
    public required bool EstaAtivo { get; init; }
}
