# Model Binding in ASP.NET Core MVC

## Overview

Model binding is the process of mapping data from HTTP requests to action method parameters in ASP.NET Core MVC. It automatically converts request data to .NET types, making it easier to work with user input.

## Basic Model Binding

### Simple Parameter Binding
```csharp
// URL: /products/details/5?category=electronics
public IActionResult Details(int id, string category)
{
    // id = 5, category = "electronics"
    return View();
}
```

### Complex Model Binding
```csharp
public class ProductModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
}

[HttpPost]
public IActionResult Create([FromForm] ProductModel product)
{
    if (!ModelState.IsValid)
    {
        return View(product);
    }
    // Process the product
    return RedirectToAction(nameof(Index));
}
```

## Binding Sources

### [FromBody] - Request Body
```csharp
public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

[HttpPost("login")]
public IActionResult Login([FromBody] LoginRequest request)
{
    // Process login request
    return Ok();
}
```

### [FromForm] - Form Data
```csharp
public class RegistrationModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}

[HttpPost]
public IActionResult Register([FromForm] RegistrationModel model)
{
    // Process registration
    return RedirectToAction("Success");
}
```

### [FromRoute] - URL Path
```csharp
[Route("orders/{year}/{month}")]
public IActionResult GetOrders(
    [FromRoute] int year,
    [FromRoute] int month)
{
    // Get orders for the specified period
    return View();
}
```

### [FromQuery] - Query String
```csharp
public class SearchParameters
{
    public string Query { get; set; }
    public string Category { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public IActionResult Search([FromQuery] SearchParameters parameters)
{
    // Perform search
    return View();
}
```

### [FromHeader] - HTTP Headers
```csharp
public IActionResult ProcessRequest(
    [FromHeader(Name = "User-Agent")] string userAgent,
    [FromHeader(Name = "X-API-Key")] string apiKey)
{
    // Use header values
    return Ok();
}
```

## Model Validation

### Data Annotations
```csharp
public class CustomerModel
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [Phone]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; }

    [Range(18, 120)]
    public int Age { get; set; }

    [CreditCard]
    public string CreditCard { get; set; }
}
```

### Custom Validation Attributes
```csharp
public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(
        object value, ValidationContext validationContext)
    {
        if (value is DateTime date)
        {
            if (date <= DateTime.Now)
            {
                return new ValidationResult("Date must be in the future");
            }
        }
        return ValidationResult.Success;
    }
}

// Usage
public class EventModel
{
    [Required]
    public string Name { get; set; }

    [FutureDate]
    public DateTime EventDate { get; set; }
}
```

### Validation in Controllers
```csharp
[HttpPost]
public IActionResult Create(CustomerModel customer)
{
    if (!ModelState.IsValid)
    {
        // Get all errors
        var errors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .Select(x => new 
            {
                Field = x.Key,
                Errors = x.Value.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList()
            })
            .ToList();

        return View(customer);
    }

    // Process valid model
    return RedirectToAction(nameof(Index));
}
```

## Custom Model Binding

### Custom Model Binder
```csharp
public class DateTimeModelBinder : IModelBinder
{
    private readonly string _customDateFormat;

    public DateTimeModelBinder(string customDateFormat)
    {
        _customDateFormat = customDateFormat;
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var modelName = bindingContext.ModelName;
        var valueProviderResult = 
            bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(
            modelName, valueProviderResult);

        var dateStr = valueProviderResult.FirstValue;

        if (string.IsNullOrEmpty(dateStr))
        {
            return Task.CompletedTask;
        }

        if (!DateTime.TryParseExact(dateStr, _customDateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None, out DateTime date))
        {
            bindingContext.ModelState.TryAddModelError(
                modelName, 
                $"DateTime should be in format {_customDateFormat}");
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(date);
        return Task.CompletedTask;
    }
}
```

### Custom Model Binder Provider
```csharp
public class DateTimeModelBinderProvider : IModelBinderProvider
{
    private readonly string _customDateFormat;

    public DateTimeModelBinderProvider(string customDateFormat)
    {
        _customDateFormat = customDateFormat;
    }

    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Metadata.ModelType == typeof(DateTime))
        {
            return new DateTimeModelBinder(_customDateFormat);
        }

        return null;
    }
}
```

### Registering Custom Model Binders
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers(options =>
    {
        options.ModelBinderProviders.Insert(0, 
            new DateTimeModelBinderProvider("dd/MM/yyyy"));
    });
}
```

## Best Practices

1. **Use Appropriate Binding Sources**
   - Match binding source to data location
   - Consider security implications
   - Use explicit binding attributes

2. **Implement Proper Validation**
   - Use data annotations
   - Add custom validation when needed
   - Handle validation errors gracefully

3. **Security Considerations**
   - Validate all input
   - Use anti-forgery tokens
   - Be cautious with binding to complex types

4. **Performance**
   - Bind only necessary properties
   - Use custom model binders for complex scenarios
   - Consider caching for expensive bindings

## Related Topics
- [Input Validation](../../4-Best-Practices/02-Security/input-validation.md)
- [Custom Model Binding](../../3-Advanced-Concepts/02-Services/custom-model-binding.md)
- [API Input Handling](../../3-Advanced-Concepts/05-API-Integration/api-input.md)