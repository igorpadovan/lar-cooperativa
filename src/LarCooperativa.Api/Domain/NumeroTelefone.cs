namespace LarCooperativa.Api.Domain;

/// <summary>
/// Normalização de números de telefone: aceita máscara comum (espaços, parênteses
/// e hífen) e produz somente os dígitos, validando a quantidade conforme o tipo.
/// </summary>
public static class NumeroTelefone
{
    public static string? TryNormalizar(string? valor, TipoTelefone tipo)
    {
        if (string.IsNullOrWhiteSpace(valor)
            || !valor.All(c => char.IsAsciiDigit(c) || c is ' ' or '(' or ')' or '-'))
        {
            return null;
        }

        var digitos = new string(valor.Where(char.IsAsciiDigit).ToArray());
        var tamanhoEsperado = tipo == TipoTelefone.Celular ? 11 : 10;

        return digitos.Length == tamanhoEsperado ? digitos : null;
    }
}
