using LarCooperativa.Api.Common;

namespace LarCooperativa.Api.Services;

public static class PessoaErrors
{
    public static readonly Error CpfInvalido = new(
        ErrorType.Validation, "cpf_invalido", "O CPF informado é inválido.");

    public static readonly Error CpfDuplicado = new(
        ErrorType.Conflict, "cpf_duplicado", "Já existe uma pessoa cadastrada com este CPF.");

    public static readonly Error NaoEncontrada = new(
        ErrorType.NotFound, "pessoa_nao_encontrada", "Pessoa não encontrada.");
}
