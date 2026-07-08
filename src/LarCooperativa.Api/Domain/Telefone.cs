namespace LarCooperativa.Api.Domain;

public enum TipoTelefone
{
    Celular,
    Residencial,
    Comercial,
}

public class Telefone
{
    public Guid Id { get; private set; }
    public Guid PessoaId { get; private set; }
    public TipoTelefone Tipo { get; private set; }

    /// <summary>Somente dígitos, com DDD (11 para celular, 10 para os demais tipos).</summary>
    public string Numero { get; private set; }

#pragma warning disable CS8618 // Construtor exigido pelo EF Core; as propriedades são materializadas do banco
    private Telefone()
    {
    }
#pragma warning restore CS8618

    public Telefone(Guid pessoaId, TipoTelefone tipo, string numero)
    {
        Id = Guid.NewGuid();
        PessoaId = pessoaId;
        Tipo = tipo;
        Numero = numero;
    }

    public void Atualizar(TipoTelefone tipo, string numero)
    {
        Tipo = tipo;
        Numero = numero;
    }
}
