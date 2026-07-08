using LarCooperativa.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LarCooperativa.Api.Data.Repositories;

public sealed class TelefoneRepository(AppDbContext context) : ITelefoneRepository
{
    public async Task<IReadOnlyList<Telefone>> GetAllByPessoaAsync(Guid pessoaId, CancellationToken cancellationToken) =>
        await context.Telefones
            .AsNoTracking()
            .Where(t => t.PessoaId == pessoaId)
            .OrderBy(t => t.Numero)
            .ToListAsync(cancellationToken);

    public Task<Telefone?> GetByIdAsync(Guid pessoaId, Guid id, CancellationToken cancellationToken) =>
        context.Telefones.FirstOrDefaultAsync(
            t => t.Id == id && t.PessoaId == pessoaId, cancellationToken);

    public Task<bool> ExistsByNumeroAsync(
        Guid pessoaId, string numero, Guid? excludeId, CancellationToken cancellationToken) =>
        context.Telefones.AnyAsync(
            t => t.PessoaId == pessoaId
                && t.Numero == numero
                && (excludeId == null || t.Id != excludeId),
            cancellationToken);

    public async Task AddAsync(Telefone telefone, CancellationToken cancellationToken)
    {
        context.Telefones.Add(telefone);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(Telefone telefone, CancellationToken cancellationToken) =>
        // A entidade já está rastreada pelo contexto (obtida via GetByIdAsync)
        context.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(Telefone telefone, CancellationToken cancellationToken)
    {
        context.Telefones.Remove(telefone);
        await context.SaveChangesAsync(cancellationToken);
    }
}
