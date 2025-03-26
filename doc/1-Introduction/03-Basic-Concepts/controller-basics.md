# Controller Basics in ASP.NET Core MVC

## Overview

Controllers are a fundamental component of ASP.NET Core MVC applications. They handle incoming HTTP requests, process user input, interact with services and models, and return appropriate responses.

## Basic Controller Structure

```csharp
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
```

## Action Methods

### Types of Action Results

```csharp
public class ProductsController : Controller
{
    // Return a view
    public IActionResult Index()
    {
        return View();
    }

    // Return JSON data
    public IActionResult GetProduct(int id)
    {
        var product = new { Id = id, Name = "Sample Product" };
        return Json(product);
    }

    // Return a file
    public IActionResult Download()
    {
        byte[] fileBytes = System.IO.File.ReadAllBytes("sample.pdf");
        return File(fileBytes, "application/pdf", "sample.pdf");
    }

    // Redirect to another action
    public IActionResult ProcessForm()
    {
        return RedirectToAction("Success");
    }

    // Return specific status code
    public IActionResult NotFound()
    {
        return NotFound();
    }

    // Return content
    public IActionResult Message()
    {
        return Content("Hello World", "text/plain");
    }
}
```

### Action Parameters

```csharp
public class OrderController : Controller
{
    // Simple parameter binding
    public IActionResult Details(int id)
    {
        return View();
    }

    // Multiple parameters
    public IActionResult Search(string category, decimal? minPrice, decimal? maxPrice)
    {
        return View();
    }

    // Complex model binding
    [HttpPost]
    public IActionResult Create([FromBody] OrderModel order)
    {
        return RedirectToAction("Index");
    }

    // Form data
    [HttpPost]
    public IActionResult Update([FromForm] ProductUpdateModel model)
    {
        return RedirectToAction("Index");
    }

    // Query string parameters
    public IActionResult Filter([FromQuery] FilterParameters parameters)
    {
        return View();
    }
}
```

## Controller Attributes

```csharp
[Authorize] // Requires authentication
[Route("api/[controller]")] // Define route template
public class ProductsApiController : ControllerBase
{
    [HttpGet] // HTTP GET method
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        return Ok();
    }

    [HttpPost] // HTTP POST method
    [ValidateAntiForgeryToken] // Prevents CSRF
    public IActionResult Create([FromBody] Product product)
    {
        return CreatedAtAction(nameof(GetById), new { id = 1 }, product);
    }

    [HttpGet("{id}")] // Route parameter
    [ResponseCache(Duration = 60)] // Enable caching
    public IActionResult GetById(int id)
    {
        return Ok();
    }
}
```

## Dependency Injection

```csharp
public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(
        ICustomerService customerService,
        ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var customers = await _customerService.GetAllAsync();
            return View(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers");
            return StatusCode(500);
        }
    }
}
```

## Model Validation

```csharp
public class AccountController : Controller
{
    [HttpPost]
    public IActionResult Register(RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Process valid model
        return RedirectToAction("Success");
    }

    [HttpPost]
    public IActionResult Update([FromBody] UserUpdateModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Custom validation
        if (!IsValidUserUpdate(model))
        {
            ModelState.AddModelError("", "Invalid user update");
            return BadRequest(ModelState);
        }

        return Ok();
    }
}
```

## Filters

```csharp
// Custom Action Filter
public class LogActionFilter : IActionFilter
{
    private readonly ILogger _logger;

    public LogActionFilter(ILogger<LogActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation($"Action {context.ActionDescriptor.DisplayName} executing");
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation($"Action {context.ActionDescriptor.DisplayName} executed");
    }
}

// Using filters in controller
[ServiceFilter(typeof(LogActionFilter))]
public class AdminController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Dashboard()
    {
        return View();
    }
}
```

## Error Handling

```csharp
public class ErrorController : Controller
{
    [Route("Error/{statusCode}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
        switch (statusCode)
        {
            case 404:
                return View("NotFound");
            case 500:
                return View("ServerError");
            default:
                return View("Error");
        }
    }

    [Route("Error")]
    public IActionResult Error()
    {
        var exceptionDetails = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };
        return View(exceptionDetails);
    }
}
```

## Best Practices

1. **Keep Controllers Focused**
   - Single responsibility principle
   - Thin controllers, fat services
   - Use dependency injection

2. **Action Method Guidelines**
   - Clear, descriptive names
   - Appropriate HTTP methods
   - Proper status codes
   - Consistent return types

3. **Security**
   - Use authorization attributes
   - Validate input
   - Protect against CSRF
   - Implement proper error handling

4. **Performance**
   - Use async/await for I/O operations
   - Implement caching where appropriate
   - Avoid excessive dependencies
   - Profile controller actions

5. **Testing**
   - Write unit tests
   - Mock dependencies
   - Test error scenarios
   - Verify model validation

## Common Patterns

### 1. CRUD Operations

```csharp
public class ProductsController : Controller
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // CREATE
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productService.CreateAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // READ
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    // UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _productService.UpdateAsync(id, model);
        return NoContent();
    }

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteAsync(id);
        return NoContent();
    }
}
```

### 2. API Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class ApiProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ApiProductsController> _logger;

    public ApiProductsController(
        IProductService productService,
        ILogger<ApiProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        try
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500);
        }
    }
}
```

## Related Topics
- [Routing Fundamentals](routing-fundamentals.md)
- [Model Binding](model-binding.md)
- [View Fundamentals](view-fundamentals.md)
- [Error Handling](../../3-Advanced-Concepts/04-Error-Handling/error-handling.md)
- [API Integration](../../3-Advanced-Concepts/05-API-Integration/api-integration.md)