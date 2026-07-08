namespace LarCooperativa.Api.Contracts;

public sealed record LoginRequest
{
    public required string Usuario { get; init; }

    public required string Senha { get; init; }
}
