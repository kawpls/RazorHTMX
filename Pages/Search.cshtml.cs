using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class SearchModel : PageModel
{
    private static readonly List<string> Properties = new()
    {
        "Luxury Beachfront Villa",
        "Modern Downtown Condo",
        "Cozy Country House",
        "Spacious Family Home",
        "High-Rise Apartment",
        "Ranch Property"
    };

    public IActionResult OnGetResults(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Content(""); // Return empty if no query
        }

        var results = Properties
            .Where(p => p.Contains(query, System.StringComparison.OrdinalIgnoreCase))
            .Select(p => $"<div class='alert alert-info'>{p}</div>")
            .ToList();

        return Content(string.Join("", results), "text/html");
    }
}
