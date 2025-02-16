using Microsoft.AspNetCore.Mvc;
using RazorHTMX.Models;

public class PropertyCardViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(PropertyViewModel property)
    {
        return View(property);
    }
}
