# ASP.NET MVC View Components

View Components are similar to partial views but more powerful, as they include both logic and view content.

## Creating View Components

### Component Class
```csharp
public class NavMenuViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        return View();
    }
}
```

### Component View
```cshtml
@model NavMenuViewModel
<nav>
    @foreach (var item in Model.MenuItems)
    {
        <a href="@item.Url">@item.Text</a>
    }
</nav>
```

## Using View Components

### In Razor Views
```cshtml
@await Component.InvokeAsync("NavMenu")
```

### With Tag Helpers
```cshtml
<vc:nav-menu></vc:nav-menu>
```

## Best Practices

1. Use for reusable UI components with logic
2. Keep components focused and single-purpose
3. Follow naming conventions
4. Implement proper dependency injection
