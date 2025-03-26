# View Fundamentals in ASP.NET Core MVC

## Introduction to Razor Views

Razor views are templates that combine C# code with HTML markup to generate dynamic web pages. They represent the 'V' in MVC (Model-View-Controller).

## Basic View Structure

### Simple View Example
```cshtml
@model ProductViewModel

<!DOCTYPE html>
<html>
<head>
    <title>@Model.Name</title>
</head>
<body>
    <h1>@Model.Name</h1>
    <p>Price: @Model.Price.ToString("C")</p>
    
    @if (Model.IsAvailable)
    {
        <button class="btn btn-primary">Add to Cart</button>
    }
    else
    {
        <p class="text-danger">Out of Stock</p>
    }
</body>
</html>
```

### View Conventions
- Views are typically stored in `/Views/{ControllerName}/{ActionName}.cshtml`
- Shared views go in `/Views/Shared/`
- Layout files are commonly placed in `/Views/Shared/_Layout.cshtml`

## Razor Syntax

### Basic Syntax
```cshtml
@* Single line expression *@
<p>Hello, @User.Identity.Name!</p>

@* Multi-line code block *@
@{
    var greeting = "Welcome";
    var currentTime = DateTime.Now;
}

@* Combining HTML and code *@
<div class="@(Model.IsActive ? "active" : "inactive")">
    @greeting, it's @currentTime.ToString("HH:mm")
</div>

@* Raw HTML output *@
@Html.Raw("<strong>Bold text</strong>")
```

### Control Structures
```cshtml
@* If statement *@
@if (User.IsInRole("Admin"))
{
    <div class="admin-panel">
        <h2>Administration</h2>
        @* Admin content *@
    </div>
}

@* For loop *@
<ul>
    @for (var i = 0; i < Model.Items.Count; i++)
    {
        <li>Item @(i + 1): @Model.Items[i].Name</li>
    }
</ul>

@* Foreach loop *@
<div class="product-grid">
    @foreach (var product in Model.Products)
    {
        <div class="product-card">
            <h3>@product.Name</h3>
            <p>@product.Description</p>
        </div>
    }
</div>

@* Switch statement *@
@switch (Model.Status)
{
    case "Active":
        <span class="badge badge-success">Active</span>
        break;
    case "Pending":
        <span class="badge badge-warning">Pending</span>
        break;
    default:
        <span class="badge badge-secondary">Unknown</span>
        break;
}
```

## Layouts and Partial Views

### Layout Page
```cshtml
@* _Layout.cshtml *@
<!DOCTYPE html>
<html>
<head>
    <title>@ViewData["Title"] - My Application</title>
    <link rel="stylesheet" href="~/css/site.css" />
</head>
<body>
    <header>
        <nav>
            <partial name="_NavbarPartial" />
        </nav>
    </header>

    <main class="container">
        @RenderBody()
    </main>

    <footer>
        <partial name="_FooterPartial" />
    </footer>

    @RenderSection("Scripts", required: false)
</body>
</html>
```

### Partial Views
```cshtml
@* _ProductCard.cshtml *@
@model ProductViewModel

<div class="card">
    <img src="@Model.ImageUrl" alt="@Model.Name" />
    <div class="card-body">
        <h5 class="card-title">@Model.Name</h5>
        <p class="card-text">@Model.Description</p>
        <p class="price">@Model.Price.ToString("C")</p>
        <partial name="_AddToCartButton" model="@Model" />
    </div>
</div>
```

## View Components

### View Component Class
```csharp
public class CartSummaryViewComponent : ViewComponent
{
    private readonly ICartService _cartService;

    public CartSummaryViewComponent(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var items = await _cartService.GetCartItemsAsync(User.Identity.Name);
        return View(items);
    }
}
```

### View Component Template
```cshtml
@* Default.cshtml in Views/Shared/Components/CartSummary *@
@model IEnumerable<CartItemViewModel>

<div class="cart-summary">
    <h4>Shopping Cart</h4>
    <ul>
        @foreach (var item in Model)
        {
            <li>@item.Name - @item.Quantity x @item.Price.ToString("C")</li>
        }
    </ul>
    <p>Total: @Model.Sum(i => i.Price * i.Quantity).ToString("C")</p>
</div>
```

### Using View Components
```cshtml
@* In any view *@
<div class="header-cart">
    @await Component.InvokeAsync("CartSummary")
</div>
```

## Tag Helpers

### Custom Tag Helper
```csharp
public class EmailTagHelper : TagHelper
{
    public string Address { get; set; }
    public bool ObfuscateEmail { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        var email = ObfuscateEmail ? ObfuscateEmailAddress(Address) : Address;
        output.Attributes.SetAttribute("href", $"mailto:{Address}");
        output.Content.SetContent(email);
    }

    private string ObfuscateEmailAddress(string email)
    {
        return email.Replace("@", " [at] ");
    }
}
```

### Using Tag Helpers
```cshtml
@* Register tag helpers at top of view or _ViewImports.cshtml *@
@addTagHelper *, MyApplication

@* Use custom tag helper *@
<email address="support@example.com" obfuscate-email="true"></email>

@* Built-in tag helpers *@
<a asp-controller="Home" asp-action="Index">Home</a>
<form asp-controller="Account" asp-action="Login" method="post">
    <input asp-for="Email" />
    <span asp-validation-for="Email"></span>
</form>
```

## ViewData and ViewBag

### Using ViewData
```cshtml
@* In Controller *@
public IActionResult Index()
{
    ViewData["Title"] = "Home Page";
    ViewData["Message"] = "Welcome to our site";
    return View();
}

@* In View *@
<h1>@ViewData["Title"]</h1>
<p>@ViewData["Message"]</p>
```

### Using ViewBag
```cshtml
@* In Controller *@
public IActionResult About()
{
    ViewBag.Title = "About Us";
    ViewBag.Description = "Learn more about our company";
    return View();
}

@* In View *@
<h1>@ViewBag.Title</h1>
<p>@ViewBag.Description</p>
```

## Best Practices

1. **Keep Views Simple**
   - Minimize logic in views
   - Use view models
   - Extract complex logic to helper methods

2. **Use Layouts Effectively**
   - Create consistent page structure
   - Share common elements
   - Define clear content sections

3. **Optimize Performance**
   - Use partial views for reusable components
   - Implement view caching where appropriate
   - Bundle and minify static resources

4. **Follow Security Guidelines**
   - Encode user input
   - Use anti-forgery tokens
   - Validate model data

## Related Topics
- [Advanced Views](../../2-Core-Components/03-Views/advanced-views.md)
- [View Components](../../2-Core-Components/03-Views/view-components.md)
- [Performance Optimization](../../4-Best-Practices/01-Performance/view-optimization.md)