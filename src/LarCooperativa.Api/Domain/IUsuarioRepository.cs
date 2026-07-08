namespace LarCooperativa.Api.Domain;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByNomeUsuarioAsync(string nomeUsuario, CancellationToken cancellationToken);

    Task AddAsync(Usuario usuario, CancellationToken cancellationToken);
}
