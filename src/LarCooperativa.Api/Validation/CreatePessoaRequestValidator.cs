using FluentValidation;
using LarCooperativa.Api.Contracts;

namespace LarCooperativa.Api.Validation;

public sealed class CreatePessoaRequestValidator : AbstractValidator<CreatePessoaRequest>
{
    public CreatePessoaRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(r => r.Nome).NomeValido();
        RuleFor(r => r.Cpf).CpfValido();
        RuleFor(r => r.DataNascimento).DataNascimentoNaoFutura(timeProvider);
    }
}
