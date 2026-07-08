using FluentValidation;
using LarCooperativa.Api.Contracts;

namespace LarCooperativa.Api.Validation;

public sealed class UpdatePessoaRequestValidator : AbstractValidator<UpdatePessoaRequest>
{
    public UpdatePessoaRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(r => r.Nome).NomeValido();
        RuleFor(r => r.Cpf).CpfValido();
        RuleFor(r => r.DataNascimento).DataNascimentoNaoFutura(timeProvider);
    }
}
