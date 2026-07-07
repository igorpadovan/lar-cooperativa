namespace LarCooperativa.Api.Domain;

public interface IPessoaRepository
{
    Task<IReadOnlyList<Pessoa>> GetAllAsync(CancellationToken cancellationToken);

    Task<Pessoa?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <param name="excludeId">Id a desconsiderar na busca (a própria pessoa, em atualizações).</param>
    Task<bool> ExistsByCpfAsync(Cpf cpf, Guid? excludeId, CancellationToken cancellationToken);

    Task AddAsync(Pessoa pessoa, CancellationToken cancellationToken);

    Task UpdateAsync(Pessoa pessoa, CancellationToken cancellationToken);

    Task DeleteAsync(Pessoa pessoa, CancellationToken cancellationToken);
}
