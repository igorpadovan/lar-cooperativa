using FluentValidation;
using LarCooperativa.Api.Contracts;

namespace LarCooperativa.Api.Validation;

public sealed class UpdateTelefoneRequestValidator : AbstractValidator<UpdateTelefoneRequest>
{
    public UpdateTelefoneRequestValidator()
    {
        RuleFor(r => r.Tipo).IsInEnum().WithMessage("Tipo de telefone desconhecido.");
        RuleFor(r => r.Numero).NumeroCompativelComTipo(r => r.Tipo);
    }
}
