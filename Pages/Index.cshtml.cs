using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Razor.Templating.Core;
using RazorHTMX.Helpers;
using RazorHTMX.Models;

public class IndexModel(IAntiforgery antiforgery, BlazorRenderer blazorRenderer) : PageModel
{
    private static readonly List<PropertyViewModel> AllProperties = GenerateProperties();
    private readonly IAntiforgery _antiforgery = antiforgery;
    private readonly BlazorRenderer _blazorRenderer = blazorRenderer;

    public required List<PropertyViewModel> Properties { get; set; }

    public required string RequestToken { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public void OnGet()
    {
        var requestTokenSet = _antiforgery.GetAndStoreTokens(HttpContext);
        if (requestTokenSet != null) RequestToken = requestTokenSet.RequestToken ?? string.Empty;

        Properties = AllProperties.Take(50).ToList(); // Load first 15
    }

    public async Task<IActionResult> OnGetListings(int pageNumber)
    {
        PageNumber = pageNumber;

        int pageSize = 50;
        var properties = AllProperties.Skip((PageNumber - 1) * pageSize).Take(pageSize).ToList();

        if (!properties.Any())
        {
            return Content(""); // No more properties to load
        }

        var nextPageUrl = Url.Page("Index", "Listings", new { pageNumber = PageNumber + 1 });

        var resultHtml = string.Join("", await Task.WhenAll(properties.Select(async (p, index) =>
        {
            var isLast = index == properties.Count - 1;
            var hxAttributes = isLast
                ? $"hx-get='{nextPageUrl}' hx-trigger='revealed' hx-swap='afterend'"
                : "";

            return $"<div class='col-md-4 mb-4' {hxAttributes}>{await RazorTemplateEngine.RenderPartialAsync("_PropertyCard", p)}</div>";
        })));

        return Content(resultHtml, "text/html");
    }



    private static List<PropertyViewModel> GenerateProperties()
    {
        var properties = new List<PropertyViewModel>();
        for (int i = 1; i <= 200; i++)
        {
            properties.Add(new PropertyViewModel
            {
                Title = $"Property {i}",
                Price = $"${100_000 + (i * 5_000)}",
                ImageUrl = "https://placehold.co/600x400",
                Inquiries = i % 7
            });
        }
        return properties;
    }

    public async Task<string> RenderPartialViewToStringAsync(string viewName, object model)
    {
        var viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine ?? throw new InvalidOperationException("View engine not found.");
        var tempData = TempData;
        var actionContext = new ActionContext(HttpContext, RouteData, PageContext.ActionDescriptor, ModelState);

        using var sw = new StringWriter();
        var viewResult = viewEngine.FindView(actionContext, viewName, false);
        if (!viewResult.Success || viewResult.View == null)
        {
            throw new InvalidOperationException($"View '{viewName}' not found. Searched locations: {string.Join(", ", viewResult.SearchedLocations)}");
        }

        var viewData = new ViewDataDictionary<object>(ViewData, model);
        var viewContext = new ViewContext(actionContext, viewResult.View, viewData, tempData, sw, new HtmlHelperOptions());
        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }
}

