namespace LarCooperativa.Api.Domain;

public class Usuario
{
    public Guid Id { get; private set; }
    public string NomeUsuario { get; private set; }
    public string SenhaHash { get; private set; }

    public Usuario(string nomeUsuario, string senhaHash)
    {
        Id = Guid.NewGuid();
        NomeUsuario = nomeUsuario;
        SenhaHash = senhaHash;
    }
}
