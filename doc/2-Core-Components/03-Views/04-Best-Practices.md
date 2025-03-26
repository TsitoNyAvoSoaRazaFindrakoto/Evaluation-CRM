# ASP.NET MVC View Best Practices

## Design Guidelines

1. **Model Usage**
   - Use strongly-typed views
   - Create view models instead of using domain models
   - Avoid business logic in views

2. **Performance**
```cshtml
@using System.Web.Optimization
@Scripts.Render("~/bundles/jquery")
@Styles.Render("~/Content/css")
```

3. **Security**
```cshtml
@Html.AntiForgeryToken()
@Html.Encode(Model.UserInput)
```

## Common Patterns

### PRG Pattern (Post-Redirect-Get)
```csharp
[HttpPost]
public IActionResult Create(CustomerViewModel model)
{
    if (ModelState.IsValid)
    {
        // Save...
        return RedirectToAction("Index");
    }
    return View(model);
}
```

### Error Handling
```cshtml
@if (!ViewData.ModelState.IsValid)
{
    @Html.ValidationSummary(true)
}
```

## Testing Views

1. Unit testing view components
2. Integration testing with Selenium
3. Using TagHelpers for testable markup
4. Implementing proper view isolation
