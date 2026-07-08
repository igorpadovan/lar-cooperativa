namespace LarCooperativa.Api.Domain;

public interface ITelefoneRepository
{
    Task<IReadOnlyList<Telefone>> GetAllByPessoaAsync(Guid pessoaId, CancellationToken cancellationToken);

    Task<Telefone?> GetByIdAsync(Guid pessoaId, Guid id, CancellationToken cancellationToken);

    /// <param name="excludeId">Id a desconsiderar na busca (o próprio telefone, em atualizações).</param>
    Task<bool> ExistsByNumeroAsync(Guid pessoaId, string numero, Guid? excludeId, CancellationToken cancellationToken);

    Task AddAsync(Telefone telefone, CancellationToken cancellationToken);

    Task UpdateAsync(Telefone telefone, CancellationToken cancellationToken);

    Task DeleteAsync(Telefone telefone, CancellationToken cancellationToken);
}
