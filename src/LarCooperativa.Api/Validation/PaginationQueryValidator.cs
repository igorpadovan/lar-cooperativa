using FluentValidation;
using LarCooperativa.Api.Contracts;

namespace LarCooperativa.Api.Validation;

public sealed class PaginationQueryValidator : AbstractValidator<PaginationQuery>
{
    public PaginationQueryValidator()
    {
        RuleFor(q => q.Pagina)
            .GreaterThanOrEqualTo(1)
            .WithMessage("A página deve ser maior ou igual a 1.");

        RuleFor(q => q.TamanhoPagina)
            .InclusiveBetween(1, PaginationQuery.TamanhoMaximo)
            .WithMessage($"O tamanho da página deve estar entre 1 e {PaginationQuery.TamanhoMaximo}.");
    }
}
