namespace LarCooperativa.Api.Contracts;

public sealed record CreatePessoaRequest
{
    public required string Nome { get; init; }

    public required string Cpf { get; init; }

    public required DateOnly DataNascimento { get; init; }
}
