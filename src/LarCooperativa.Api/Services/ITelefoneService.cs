using LarCooperativa.Api.Common;
using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Domain;

namespace LarCooperativa.Api.Services;

public interface ITelefoneService
{
    Task<Result<IReadOnlyList<Telefone>>> GetAllAsync(Guid pessoaId, CancellationToken cancellationToken);

    Task<Result<Telefone>> GetByIdAsync(Guid pessoaId, Guid id, CancellationToken cancellationToken);

    Task<Result<Telefone>> CreateAsync(Guid pessoaId, CreateTelefoneRequest request, CancellationToken cancellationToken);

    Task<Result<Telefone>> UpdateAsync(Guid pessoaId, Guid id, UpdateTelefoneRequest request, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid pessoaId, Guid id, CancellationToken cancellationToken);
}
