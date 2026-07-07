namespace LarCooperativa.Api.Domain;

public class Pessoa
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public Cpf Cpf { get; private set; }
    public DateOnly DataNascimento { get; private set; }
    public bool EstaAtivo { get; private set; }

#pragma warning disable CS8618 // Construtor exigido pelo EF Core; as propriedades são materializadas do banco
    private Pessoa()
    {
    }
#pragma warning restore CS8618

    public Pessoa(string nome, Cpf cpf, DateOnly dataNascimento)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Cpf = cpf;
        DataNascimento = dataNascimento;
        EstaAtivo = true;
    }

    public void Atualizar(string nome, Cpf cpf, DateOnly dataNascimento, bool estaAtivo)
    {
        Nome = nome;
        Cpf = cpf;
        DataNascimento = dataNascimento;
        EstaAtivo = estaAtivo;
    }
}
