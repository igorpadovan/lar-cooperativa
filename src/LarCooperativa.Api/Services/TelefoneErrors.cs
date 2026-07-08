using LarCooperativa.Api.Common;

namespace LarCooperativa.Api.Services;

public static class TelefoneErrors
{
    public static readonly Error NumeroInvalido = new(
        ErrorType.Validation,
        "numero_invalido",
        "O número é inválido: informe DDD + número (11 dígitos para celular, 10 para os demais tipos).");

    public static readonly Error NumeroDuplicado = new(
        ErrorType.Conflict, "numero_duplicado", "A pessoa já possui este número de telefone.");

    public static readonly Error NaoEncontrado = new(
        ErrorType.NotFound, "telefone_nao_encontrado", "Telefone não encontrado.");
}
