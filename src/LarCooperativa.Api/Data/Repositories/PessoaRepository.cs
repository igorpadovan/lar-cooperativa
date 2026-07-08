using LarCooperativa.Api.Common;
using LarCooperativa.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LarCooperativa.Api.Data.Repositories;

public sealed class PessoaRepository(AppDbContext context) : IPessoaRepository
{
    public async Task<Page<Pessoa>> GetPageAsync(int pagina, int tamanhoPagina, CancellationToken cancellationToken)
    {
        var totalItens = await context.Pessoas.CountAsync(cancellationToken);
        var itens = await context.Pessoas
            .AsNoTracking()
            // Desempate por Id para a ordenação (e as páginas) serem estáveis
            .OrderBy(p => p.Nome).ThenBy(p => p.Id)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        return new Page<Pessoa>(itens, pagina, tamanhoPagina, totalItens);
    }

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
