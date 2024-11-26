using FiltroKMLApi.Models;
using FluentValidation;

public class PlacemarkFiltroValidador : AbstractValidator<PlacemarkFiltro>
{
    public PlacemarkFiltroValidador()
    {
        RuleFor(f => f.Referencia)
            .MinimumLength(3)
            .When(f => !string.IsNullOrWhiteSpace(f.Referencia))
            .WithMessage("O campo 'Referência' deve ter no mínimo 3 caracteres.");

        RuleFor(f => f.RuaCruzamento)
            .MinimumLength(3)
            .When(f => !string.IsNullOrWhiteSpace(f.RuaCruzamento))
            .WithMessage("O campo 'Rua/Cruzamento' deve ter no mínimo 3 caracteres.");

    }
}
