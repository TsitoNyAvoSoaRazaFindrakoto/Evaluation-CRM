# Model Binding in ASP.NET MVC

Model binding automatically maps HTTP request data to action method parameters and model objects.

## Basic Model Binding

### Action Parameters
```csharp
public IActionResult Edit(int id, string name)
{
    // Parameters automatically bound from route, query string, or form data
}
```

### Complex Types
```csharp
public class CustomerViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public IActionResult Create([FromBody] CustomerViewModel customer)
{
    // Model automatically bound from request body
}
```

## Binding Sources

- [FromQuery] - Query string
- [FromRoute] - Route data
- [FromForm] - Form data
- [FromBody] - Request body
- [FromHeader] - Request headers

## Custom Model Binding

```csharp
public class CustomModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        // Custom binding logic
    }
}
```
