# Model Binding in ASP.NET Core MVC

## Overview
Model binding is the process of mapping data from HTTP requests to action method parameters and properties of model objects. It handles:
- Form data
- Route values
- Query string parameters
- HTTP headers
- JSON/XML request bodies

## Basic Model Binding

### Simple Types
```csharp
public class ProductController : Controller
{
    // Binds from route: /Product/Details/5
    public IActionResult Details(int id)
    {
        // id is bound from route value
        return View();
    }

    // Binds from query string: /Product/Search?name=xbox&maxPrice=500
    public IActionResult Search(string name, decimal maxPrice)
    {
        // Parameters bound from query string
        return View();
    }
}
```

### Complex Types
```csharp
public class SearchCriteria
{
    public string Name { get; set; }
    public decimal? MaxPrice { get; set; }
    public string Category { get; set; }
    public bool InStock { get; set; }
}

public class ProductController : Controller
{
    // Binds from query string: /Product/Search?Name=xbox&MaxPrice=500&Category=Games
    public IActionResult Search(SearchCriteria criteria)
    {
        // criteria object is populated from query string
        return View(criteria);
    }
}
```

## Advanced Binding

### Form Data Binding
```csharp
public class ProductController : Controller
{
    [HttpPost]
    public IActionResult Create([FromForm] CreateProductViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Process the form data
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }
}

public class CreateProductViewModel
{
    [Required]
    public string Name { get; set; }

    [Required]
    public decimal Price { get; set; }

    // Handles file upload
    [Display(Name = "Product Image")]
    public IFormFile ImageFile { get; set; }

    // Handles multiple select
    public List<int> CategoryIds { get; set; }
}
```

### JSON Binding
```csharp
[ApiController]
public class ApiProductController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] ProductDto product)
    {
        if (ModelState.IsValid)
        {
            // Process the JSON data
            return Ok(product);
        }
        return BadRequest(ModelState);
    }
}

public class ProductDto
{
    [JsonPropertyName("productName")]
    public string Name { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal Price { get; set; }

    [JsonPropertyName("categories")]
    public string[] Categories { get; set; }
}
```

## Custom Model Binding

### Custom Model Binder
```csharp
public class DateTimeModelBinder : IModelBinder
{
    public Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
            throw new ArgumentNullException(nameof(bindingContext));

        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None)
            return Task.FromResult(ModelBindingResult.Failed());

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
        var value = valueProviderResult.FirstValue;

        if (string.IsNullOrEmpty(value))
            return Task.FromResult(ModelBindingResult.Failed());

        // Custom date parsing logic
        if (DateTime.TryParseExact(value, 
            new[] { "dd/MM/yyyy", "dd-MM-yyyy" },
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime datetime))
        {
            return Task.FromResult(ModelBindingResult.Success(datetime));
        }

        bindingContext.ModelState.TryAddModelError(modelName, 
            "Date should be in dd/MM/yyyy or dd-MM-yyyy format");
        return Task.FromResult(ModelBindingResult.Failed());
    }
}
```

### Using Custom Binder
```csharp
public class AppointmentViewModel
{
    [Required]
    public string Title { get; set; }

    [Required]
    [ModelBinder(typeof(DateTimeModelBinder))]
    public DateTime AppointmentDate { get; set; }
}

// Or apply to a parameter
public IActionResult Schedule(
    [ModelBinder(typeof(DateTimeModelBinder))] DateTime date)
{
    return View();
}
```

## Binding Sources

### Source Attributes
```csharp
public class OrderController : Controller
{
    [HttpPost]
    public IActionResult Process(
        [FromRoute] int id,
        [FromBody] OrderDetails details,
        [FromQuery] string referrer,
        [FromHeader] string authorization,
        [FromForm] IFormFile attachment)
    {
        // Each parameter bound from specific source
        return Ok();
    }
}
```

### Custom Value Provider
```csharp
public class CookieValueProvider : IValueProvider
{
    private readonly IRequestCookieCollection _cookies;

    public CookieValueProvider(IRequestCookieCollection cookies)
    {
        _cookies = cookies;
    }

    public bool ContainsPrefix(string prefix)
    {
        return _cookies.Any(c => c.Key.StartsWith(prefix));
    }

    public ValueProviderResult GetValue(string key)
    {
        if (_cookies.TryGetValue(key, out string value))
        {
            return new ValueProviderResult(value);
        }
        return ValueProviderResult.None;
    }
}
```

## Collections and Arrays

### Array Binding
```csharp
public class OrderViewModel
{
    public int[] ProductIds { get; set; }
    public List<int> Quantities { get; set; }
    public Dictionary<string, decimal> PriceOverrides { get; set; }
}

// Form data:
// ProductIds[0]=1&ProductIds[1]=2
// Quantities[0]=3&Quantities[1]=4
// PriceOverrides[product1]=10.99&PriceOverrides[product2]=20.99
```

### Complex Collections
```csharp
public class OrderLineItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class OrderViewModel
{
    public List<OrderLineItem> Items { get; set; }
}

// Form data:
// Items[0].ProductId=1&Items[0].Quantity=2&Items[0].UnitPrice=9.99
// Items[1].ProductId=2&Items[1].Quantity=1&Items[1].UnitPrice=19.99
```

## Best Practices

### 1. Security
- Validate bound data
- Use anti-forgery tokens
- Prevent over-posting
- Sanitize inputs

### 2. Performance
- Use appropriate binding source
- Avoid binding unnecessary data
- Consider custom binders for complex scenarios
- Cache compiled model metadata

### 3. Error Handling
- Provide clear validation messages
- Handle missing/invalid data gracefully
- Log binding failures
- Return appropriate status codes

### 4. Design
- Keep models focused
- Use view models for forms
- Implement custom binders when needed
- Consider validation requirements

## Common Issues and Solutions

### Over-posting Prevention
```csharp
// Using [Bind] attribute
[HttpPost]
public IActionResult Create([Bind("Name,Price,Description")] Product product)
{
    // Only specified properties will be bound
    return View(product);
}

// Using view models
public class CreateProductViewModel
{
    // Only include properties that should be bound
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}
```

### Handling Null Values
```csharp
public class FilterModel
{
    // Use nullable types for optional parameters
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Provide default values
    public int PageSize { get; set; } = 10;
    public int PageNumber { get; set; } = 1;
}
```

## Related Topics
- [Model Validation](model-validation.md)
- [Forms and HTML Helpers](../03-Views/forms-and-html.md)
- [API Controllers](../02-Controllers/api-controllers.md)