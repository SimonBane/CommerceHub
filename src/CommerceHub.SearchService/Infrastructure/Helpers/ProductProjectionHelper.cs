namespace CommerceHub.SearchService.Infrastructure.Helpers;

public static class ProductProjectionHelper
{
    internal static string[] BuildSearchTerms(string name, string description, string sku)
    {
        var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var word in name.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            terms.Add(word.ToLowerInvariant());
        foreach (var word in description.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            if (word.Length > 2) terms.Add(word.ToLowerInvariant());
        terms.Add(sku.ToLowerInvariant());
        return terms.ToArray();
    }
}