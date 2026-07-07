using LarCooperativa.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LarCooperativa.Api.Data.Repositories;

public sealed class PessoaRepository(AppDbContext context) : IPessoaRepository
{
    public async Task<IReadOnlyList<Pessoa>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.Pessoas
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);

    public Task<Pessoa?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Pessoas.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> ExistsByCpfAsync(Cpf cpf, Guid? excludeId, CancellationToken cancellationToken) =>
        context.Pessoas.AnyAsync(
            p => p.Cpf == cpf && (excludeId == null || p.Id != excludeId),
            cancellationToken);

    public async Task AddAsync(Pessoa pessoa, CancellationToken cancellationToken)
    {
        context.Pessoas.Add(pessoa);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(Pessoa pessoa, CancellationToken cancellationToken) =>
        // A entidade já está rastreada pelo contexto (obtida via GetByIdAsync)
        context.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(Pessoa pessoa, CancellationToken cancellationToken)
    {
        context.Pessoas.Remove(pessoa);
        await context.SaveChangesAsync(cancellationToken);
    }
}
