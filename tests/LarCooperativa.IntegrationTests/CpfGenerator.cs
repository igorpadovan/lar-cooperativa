namespace LarCooperativa.IntegrationTests;

/// <summary>
/// Gera CPFs válidos e únicos para que os testes não interfiram entre si.
/// </summary>
internal static class CpfGenerator
{
    public static string Valido()
    {
        var digitos = new int[11];
        for (var i = 0; i < 9; i++)
        {
            digitos[i] = Random.Shared.Next(0, 10);
        }

        digitos[9] = CalcularDigitoVerificador(digitos, 9);
        digitos[10] = CalcularDigitoVerificador(digitos, 10);

        var cpf = string.Concat(digitos);
        return cpf.Distinct().Count() == 1 ? Valido() : cpf;
    }

    private static int CalcularDigitoVerificador(int[] digitos, int posicao)
    {
        var soma = 0;
        for (var i = 0; i < posicao; i++)
        {
            soma += digitos[i] * (posicao + 1 - i);
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
