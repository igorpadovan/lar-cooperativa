using FluentValidation;
using LarCooperativa.Api.Contracts;

namespace LarCooperativa.Api.Validation;

public sealed class CreateTelefoneRequestValidator : AbstractValidator<CreateTelefoneRequest>
{
    public CreateTelefoneRequestValidator()
    {
        RuleFor(r => r.Tipo).IsInEnum().WithMessage("Tipo de telefone desconhecido.");
        RuleFor(r => r.Numero).NumeroCompativelComTipo(r => r.Tipo);
    }
}
