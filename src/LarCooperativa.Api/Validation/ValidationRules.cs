using FluentValidation;
using LarCooperativa.Api.Domain;

namespace LarCooperativa.Api.Validation;

/// <summary>
/// Regras de validação compartilhadas entre os validators de create/update.
/// </summary>
public static class ValidationRules
{
    public static IRuleBuilderOptions<T, string> NomeValido<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(200).WithMessage("O nome deve ter no máximo 200 caracteres.");

    public static IRuleBuilderOptions<T, string> CpfValido<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .Must(valor => Cpf.TryCreate(valor) is not null)
            .WithMessage("O CPF informado é inválido.");

    public static IRuleBuilderOptions<T, DateOnly> DataNascimentoNaoFutura<T>(
        this IRuleBuilder<T, DateOnly> ruleBuilder, TimeProvider timeProvider) =>
        ruleBuilder
            .Must(data => data <= DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime))
            .WithMessage("A data de nascimento não pode estar no futuro.");

    public static IRuleBuilderOptions<T, string> NumeroCompativelComTipo<T>(
        this IRuleBuilder<T, string> ruleBuilder, Func<T, TipoTelefone> tipo) =>
        ruleBuilder
            .Must((request, numero) => NumeroTelefone.TryNormalizar(numero, tipo(request)) is not null)
            .WithMessage("O número é inválido: informe DDD + número (11 dígitos para celular, 10 para os demais tipos).");
}
