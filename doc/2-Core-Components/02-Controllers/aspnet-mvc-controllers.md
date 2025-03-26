# ASP.NET MVC Controllers

## Overview
Controllers in ASP.NET MVC are classes that handle incoming HTTP requests, process user input, and return responses. They serve as the 'C' in the MVC (Model-View-Controller) pattern, managing the flow between the user interface and data layers.

## Key Characteristics

### Base Class
Controllers typically inherit from `Controller` base class:
```csharp
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
```

### Action Methods
Action methods are public methods in a controller that respond to HTTP requests:
```csharp
public class ProductController : Controller
{
    public IActionResult Details(int id)
    {
        var product = _productService.GetById(id);
        return View(product);
    }
}
```

## Common Return Types

### IActionResult Types
- `ViewResult` (View())
- `JsonResult` (Json())
- `ContentResult` (Content())
- `RedirectResult` (Redirect())
- `FileResult` (File())

```csharp
public IActionResult Example()
{
    // Return view
    return View();
    
    // Return JSON
    return Json(new { success = true });
    
    // Return plain text
    return Content("Hello World");
    
    // Redirect
    return RedirectToAction("Index");
}
```

## Parameter Binding

### From Route
```csharp
[Route("products/{id}")]
public IActionResult GetProduct(int id)
{
    return View(_productService.GetById(id));
}
```

### From Query String
```csharp
public IActionResult Search([FromQuery]string term)
{
    var results = _searchService.Search(term);
    return View(results);
}
```

### From Form
```csharp
[HttpPost]
public IActionResult Create([FromForm]ProductViewModel model)
{
    if (ModelState.IsValid)
    {
        _productService.Create(model);
        return RedirectToAction("Index");
    }
    return View(model);
}
```

## Attributes

### HTTP Method Attributes
```csharp
[HttpGet]
[HttpPost]
[HttpPut]
[HttpDelete]
public IActionResult ActionName() { }
```

### Route Attributes
```csharp
[Route("api/[controller]")]
[Route("[controller]/[action]")]
```

### Authorization Attributes
```csharp
[Authorize]
[AllowAnonymous]
```

## Dependency Injection
```csharp
public class OrderController : Controller
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }
}
```

## Best Practices

### 1. Keep Controllers Thin
- Move business logic to services
- Use ViewModels for data transfer
- Handle only HTTP concerns

### 2. Proper Error Handling
```csharp
public IActionResult GetOrder(int id)
{
    try
    {
        var order = _orderService.GetById(id);
        if (order == null)
            return NotFound();
            
        return View(order);
    }
    catch (Exception ex)
    {
        return StatusCode(500, "Internal server error");
    }
}
```

### 3. Model Validation
```csharp
[HttpPost]
public IActionResult Create(ProductViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
        
    // Process valid model
}
```

### 4. Action Filters
```csharp
[CustomActionFilter]
public IActionResult Action()
{
    // Action logic
}
```

## Testing
```csharp
[Fact]
public void Index_ReturnsViewResult()
{
    // Arrange
    var controller = new HomeController();

    // Act
    var result = controller.Index();

    // Assert
    Assert.IsType<ViewResult>(result);
}
```

## Security Considerations
- Use Anti-forgery tokens
- Implement proper authorization
- Validate input
- Use HTTPS
- Implement proper authentication

## Performance Tips
- Use async/await for I/O operations
- Implement caching where appropriate
- Use proper status codes
- Optimize database queries
- Use appropriate return types

## Common Patterns

### PRG (Post-Redirect-Get)
```csharp
[HttpPost]
public IActionResult Create(ProductViewModel model)
{
    if (ModelState.IsValid)
    {
        _productService.Create(model);
        return RedirectToAction("Index");
    }
    return View(model);
}
```

### API Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductApiController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Product>> Get()
    {
        return Ok(_productService.GetAll());
    }
}
```

## Conclusion
ASP.NET MVC Controllers are crucial components that require careful design and implementation. Following these practices ensures maintainable, secure, and efficient applications.
