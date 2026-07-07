namespace LarCooperativa.Api.Domain;

/// <summary>
/// Value object de CPF: armazena somente os 11 dígitos, já validados.
/// </summary>
public sealed record Cpf
{
    public string Valor { get; }

    private Cpf(string valor) => Valor = valor;

    /// <summary>
    /// Cria um CPF a partir de uma entrada com ou sem máscara; retorna null se inválido.
    /// </summary>
    public static Cpf? TryCreate(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var digitos = valor.Trim().Replace(".", string.Empty).Replace("-", string.Empty);
        if (digitos.Length != 11 || !digitos.All(char.IsAsciiDigit))
        {
            return null;
        }

        // CPFs com todos os dígitos iguais passam no cálculo, mas não são válidos
        if (digitos.Distinct().Count() == 1)
        {
            return null;
        }

        return DigitosVerificadoresValidos(digitos) ? new Cpf(digitos) : null;
    }

    /// <summary>
    /// Reconstrói o value object a partir de um valor já validado (ex.: vindo do banco).
    /// </summary>
    internal static Cpf FromTrusted(string valor) => new(valor);

    public override string ToString() => Valor;

    private static bool DigitosVerificadoresValidos(string cpf) =>
        CalcularDigitoVerificador(cpf, 9) == cpf[9] - '0'
        && CalcularDigitoVerificador(cpf, 10) == cpf[10] - '0';

    private static int CalcularDigitoVerificador(string cpf, int posicao)
    {
        var soma = 0;
        for (var i = 0; i < posicao; i++)
        {
            soma += (cpf[i] - '0') * (posicao + 1 - i);
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
