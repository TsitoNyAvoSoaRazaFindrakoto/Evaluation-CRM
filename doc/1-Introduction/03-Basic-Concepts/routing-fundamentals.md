# Routing in ASP.NET Core MVC

## Overview

Routing is the process of mapping URLs to controller actions in ASP.NET Core MVC. It's a fundamental concept that determines how your application responds to HTTP requests.

## Route Types

### 1. Convention-based Routing

```csharp
// Program.cs
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
```

### 2. Attribute Routing

```csharp
[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_products);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        return Ok(_products.FirstOrDefault(p => p.Id == id));
    }

    [HttpGet("category/{category}")]
    public IActionResult GetByCategory(string category)
    {
        return Ok(_products.Where(p => p.Category == category));
    }
}
```

## Route Templates

### Basic Templates
```csharp
// Simple route
app.MapControllerRoute(
    name: "blog",
    pattern: "blog/{*article}",
    defaults: new { controller = "Blog", action = "Article" });

// Route with defaults
app.MapControllerRoute(
    name: "shop",
    pattern: "shop/{category}/{page?}",
    defaults: new { controller = "Shop", action = "Browse", page = 1 });

// Route with constraints
app.MapControllerRoute(
    name: "archive",
    pattern: "archive/{year}/{month}",
    defaults: new { controller = "Archive", action = "Index" },
    constraints: new { year = @"\d{4}", month = @"\d{2}" });
```

### Advanced Templates
```csharp
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class OrdersController : ControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public IActionResult GetV1()
    {
        return Ok("Version 1.0");
    }

    [HttpGet]
    [MapToApiVersion("2.0")]
    public IActionResult GetV2()
    {
        return Ok("Version 2.0");
    }
}
```

## Route Constraints

### Built-in Constraints

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapControllerRoute(
            name: "constraint_examples",
            pattern: "{controller}/{action}/{id:int?}");

        app.MapControllerRoute(
            name: "guid_route",
            pattern: "api/items/{id:guid}",
            defaults: new { controller = "Items", action = "GetById" });

        app.MapControllerRoute(
            name: "date_route",
            pattern: "events/{date:datetime}",
            defaults: new { controller = "Events", action = "GetByDate" });

        app.MapControllerRoute(
            name: "alpha_route",
            pattern: "users/{username:alpha}",
            defaults: new { controller = "Users", action = "Profile" });
    }
}
```

### Custom Constraints

```csharp
public class EvenNumberConstraint : IRouteConstraint
{
    public bool Match(
        HttpContext httpContext,
        IRouter route,
        string routeKey,
        RouteValueDictionary values,
        RouteDirection routeDirection)
    {
        if (!values.TryGetValue(routeKey, out var value))
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        return int.TryParse(value.ToString(), out var number) && number % 2 == 0;
    }
}

// Registration in Program.cs
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<RouteOptions>(options =>
        {
            options.ConstraintMap.Add("even", typeof(EvenNumberConstraint));
        });

        var app = builder.Build();

        app.MapControllerRoute(
            name: "even_numbers",
            pattern: "numbers/{id:even}",
            defaults: new { controller = "Numbers", action = "GetEven" });
    }
}
```

## URL Generation

### 1. Using IUrlHelper

```csharp
public class NavigationController : Controller
{
    public IActionResult GenerateLinks()
    {
        // Generate URL by action and controller
        var url1 = Url.Action("Index", "Home");

        // Generate URL with route values
        var url2 = Url.Action("Details", "Products", new { id = 123 });

        // Generate URL by route name
        var url3 = Url.RouteUrl("blog", new { year = 2025, month = 03 });

        // Generate absolute URL
        var url4 = Url.Action("Index", "Home", null, Request.Scheme);

        return View(new
        {
            HomeUrl = url1,
            ProductUrl = url2,
            BlogUrl = url3,
            AbsoluteUrl = url4
        });
    }
}
```

### 2. Using Tag Helpers

```html
<!-- Basic link -->
<a asp-controller="Home" asp-action="Index">Home</a>

<!-- Link with route values -->
<a asp-controller="Products" 
   asp-action="Details" 
   asp-route-id="@Model.Id">
    View Product
</a>

<!-- Link with route name -->
<a asp-route="blog" 
   asp-route-year="2025" 
   asp-route-month="03">
    March 2025 Blog
</a>

<!-- Area link -->
<a asp-area="Admin" 
   asp-controller="Dashboard" 
   asp-action="Index">
    Admin Dashboard
</a>
```

### 3. Using HTML Helpers

```html
@* Basic link *@
@Html.ActionLink("Home", "Index", "Home")

@* Link with route values *@
@Html.ActionLink("View Product", "Details", "Products", new { id = Model.Id })

@* Link with route name *@
@Html.RouteLink("March 2025 Blog", "blog", new { year = 2025, month = 03 })

@* Raw URL generation *@
@Url.Action("Index", "Home")
```

## Route Groups

```csharp
app.MapGroup("/api/v1")
    .MapGroup("/products")
    .WithTags("Products")
    .RequireAuthorization()
    .AddEndpointFilter<ApiKeyFilter>()
    .Map("/", () => Results.Ok())
    .Map("/{id}", (int id) => Results.Ok())
    .Map("/search", (string query) => Results.Ok());
```

## Best Practices

1. **Route Organization**
   - Keep routes simple and intuitive
   - Use meaningful names
   - Avoid deep nesting
   - Consider URL SEO

2. **Security**
   - Validate route parameters
   - Use constraints appropriately
   - Consider authorization requirements

3. **Performance**
   - Order routes correctly (most specific first)
   - Use route constraints efficiently
   - Cache generated URLs when appropriate

4. **Maintainability**
   - Use attribute routing for APIs
   - Use conventional routing for MVC
   - Document complex routing patterns
   - Use route templates consistently

## Common Patterns

### 1. RESTful Routes

```csharp
[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok();

    [HttpGet("{id}")]
    public IActionResult GetById(int id) => Ok();

    [HttpPost]
    public IActionResult Create([FromBody] Product product) => Ok();

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Product product) => Ok();

    [HttpDelete("{id}")]
    public IActionResult Delete(int id) => Ok();
}
```

### 2. Area Routes

```csharp
[Area("Admin")]
public class DashboardController : Controller
{
    [Route("")]
    [Route("admin")]
    [Route("admin/dashboard")]
    public IActionResult Index()
    {
        return View();
    }

    [Route("admin/dashboard/stats")]
    public IActionResult Statistics()
    {
        return View();
    }
}
```

### 3. Localized Routes

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{culture}/{controller=Home}/{action=Index}/{id?}",
    constraints: new { culture = new CultureConstraint(new[] { "en", "fr", "es" }) });

public class CultureConstraint : IRouteConstraint
{
    private readonly string[] _validCultures;

    public CultureConstraint(string[] validCultures)
    {
        _validCultures = validCultures;
    }

    public bool Match(
        HttpContext httpContext,
        IRouter route,
        string routeKey,
        RouteValueDictionary values,
        RouteDirection routeDirection)
    {
        if (!values.TryGetValue(routeKey, out var value))
        {
            return false;
        }

        var culture = value?.ToString();
        return !string.IsNullOrEmpty(culture) && 
               _validCultures.Contains(culture.ToLowerInvariant());
    }
}
```

## Related Topics
- [Controller Basics](controller-basics.md)
- [URL Generation](../../2-Core-Components/01-Models/url-generation.md)
- [Route Security](../../4-Best-Practices/02-Security/route-security.md)
- [API Design](../../3-Advanced-Concepts/05-API-Integration/api-design.md)