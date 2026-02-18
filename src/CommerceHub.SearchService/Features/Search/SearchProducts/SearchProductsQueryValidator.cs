using FluentValidation;

namespace CommerceHub.SearchService.Features.Search.SearchProducts;

public sealed class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
{
    public SearchProductsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Query).MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Query));
    }
}
