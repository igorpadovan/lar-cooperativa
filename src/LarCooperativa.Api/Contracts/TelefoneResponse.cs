using LarCooperativa.Api.Domain;

namespace LarCooperativa.Api.Contracts;

public sealed record TelefoneResponse(
    Guid Id,
    Guid PessoaId,
    TipoTelefone Tipo,
    string Numero)
{
    public static TelefoneResponse FromDomain(Telefone telefone) => new(
        telefone.Id,
        telefone.PessoaId,
        telefone.Tipo,
        telefone.Numero);
}
