using LarCooperativa.Api.Domain;

namespace LarCooperativa.Api.Contracts;

public sealed record PessoaResponse(
    Guid Id,
    string Nome,
    string Cpf,
    DateOnly DataNascimento,
    bool EstaAtivo)
{
    public static PessoaResponse FromDomain(Pessoa pessoa) => new(
        pessoa.Id,
        pessoa.Nome,
        pessoa.Cpf.Valor,
        pessoa.DataNascimento,
        pessoa.EstaAtivo);
}
