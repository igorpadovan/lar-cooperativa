using LarCooperativa.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LarCooperativa.Api.Data.Repositories;

public sealed class UsuarioRepository(AppDbContext context) : IUsuarioRepository
{
    public Task<Usuario?> GetByNomeUsuarioAsync(string nomeUsuario, CancellationToken cancellationToken) =>
        context.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u => u.NomeUsuario == nomeUsuario, cancellationToken);

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync(cancellationToken);
    }
}
