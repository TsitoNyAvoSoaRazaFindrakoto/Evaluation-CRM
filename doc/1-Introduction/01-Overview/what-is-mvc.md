# What is ASP.NET Core MVC?

## Definition
ASP.NET Core MVC is a web application framework that implements the Model-View-Controller (MVC) architectural pattern using ASP.NET Core, the cross-platform version of ASP.NET.

## Key Characteristics

### Cross-Platform
- Runs on Windows, Linux, and macOS
- Supports multiple deployment environments
- Compatible with various IDEs (Visual Studio, VS Code, Rider)

### Modern Architecture
- Built on .NET Core/.NET 5+
- Modular design
- High performance
- Cloud-ready

### Open Source
- Maintained by Microsoft and the community
- Available on GitHub
- MIT licensed

## MVC Pattern Explained

### Model
- Represents business logic and data
- Handles data validation
- Implements business rules
- Examples:
```csharp
public class Product
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

### View
- Handles UI presentation
- Uses Razor syntax
- Renders HTML content
- Example:
```cshtml
@model Product
<h1>@Model.Name</h1>
<p>Price: @Model.Price.ToString("C")</p>
```

### Controller
- Processes user input
- Manages application flow
- Coordinates between Model and View
- Example:
```csharp
public class ProductsController : Controller
{
    public IActionResult Index()
    {
        var products = _productService.GetAll();
        return View(products);
    }
}
```

## Core Benefits

### Separation of Concerns
- Clear division of responsibilities
- Improved maintainability
- Better testability
- Easier parallel development

### Developer Productivity
- Convention over configuration
- Built-in dependency injection
- Rich ecosystem of tools and packages
- Comprehensive testing support

### Enterprise Features
- Built-in security
- Performance optimization
- Caching capabilities
- Extensible architecture

## Further Reading
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Source Code on GitHub](https://github.com/dotnet/aspnetcore)
- [MVC Design Pattern](../03-Basic-Concepts/design-patterns.md)