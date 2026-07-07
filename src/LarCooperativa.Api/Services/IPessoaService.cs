using LarCooperativa.Api.Common;
using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Domain;

namespace LarCooperativa.Api.Services;

public interface IPessoaService
{
    Task<IReadOnlyList<Pessoa>> GetAllAsync(CancellationToken cancellationToken);

    Task<Pessoa?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<Pessoa>> CreateAsync(CreatePessoaRequest request, CancellationToken cancellationToken);

    Task<Result<Pessoa>> UpdateAsync(Guid id, UpdatePessoaRequest request, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
