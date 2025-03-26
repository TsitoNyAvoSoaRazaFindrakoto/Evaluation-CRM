# Request Lifecycle in ASP.NET Core MVC

## Overview

Understanding the request lifecycle is crucial for developing ASP.NET Core MVC applications. This document explains how a request flows through the application, from initial receipt to final response.

## High-Level Flow

1. Request arrives at the server
2. Middleware pipeline processes the request
3. Routing determines the appropriate controller and action
4. Model binding processes request data
5. Action filters run
6. Action method executes
7. Result execution
8. Response sent to client

## Detailed Pipeline

### 1. Request Reception
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Configure the HTTP request pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

### 2. Middleware Pipeline

#### Custom Middleware Example
```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            _logger.LogInformation(
                "Request {method} {url} started", 
                context.Request.Method, 
                context.Request.Path);

            var sw = Stopwatch.StartNew();
            await _next(context);
            sw.Stop();

            _logger.LogInformation(
                "Request {method} {url} completed in {elapsed}ms",
                context.Request.Method,
                context.Request.Path,
                sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Request {method} {url} failed",
                context.Request.Method,
                context.Request.Path);
            throw;
        }
    }
}

// Extension method for cleaner registration
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
```

### 3. Routing Process

#### Route Registration
```csharp
app.MapControllerRoute(
    name: "blog",
    pattern: "blog/{year}/{month}/{slug}",
    defaults: new { controller = "Blog", action = "Post" },
    constraints: new
    {
        year = @"\d{4}",
        month = @"\d{2}",
        slug = @"[a-z0-9-]+"
    });

app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

### 4. Model Binding Process

#### Custom Model Binder
```csharp
public class CommaSeparatedArrayModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = 
            bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        var values = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
        bindingContext.Result = ModelBindingResult.Success(values);

        return Task.CompletedTask;
    }
}

// Usage in controller
public IActionResult Filter([ModelBinder(typeof(CommaSeparatedArrayModelBinder))] string[] tags)
{
    // tags will be bound from query string like "?tags=asp,net,core"
    return View(tags);
}
```

### 5. Action Filters

#### Filter Pipeline
```csharp
public class AuditAttribute : ActionFilterAttribute
{
    private readonly ILogger _logger;

    public AuditAttribute(ILogger<AuditAttribute> logger)
    {
        _logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User.Identity?.Name ?? "anonymous";
        var action = context.ActionDescriptor.DisplayName;
        var parameters = string.Join(", ", context.ActionArguments
            .Select(x => $"{x.Key}={x.Value}"));

        _logger.LogInformation(
            "User {user} executing {action} with parameters: {parameters}",
            user, action, parameters);

        base.OnActionExecuting(context);
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var user = context.HttpContext.User.Identity?.Name ?? "anonymous";
        var action = context.ActionDescriptor.DisplayName;
        var result = context.Result;

        _logger.LogInformation(
            "User {user} completed {action} with result: {result}",
            user, action, result?.GetType().Name);

        base.OnActionExecuted(context);
    }
}
```

### 6. Action Execution

#### Action Method Lifecycle
```csharp
[Audit]
[ValidateModel]
public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(
        IProductService productService,
        ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductModel model)
    {
        try
        {
            var product = await _productService.CreateAsync(model);
            _logger.LogInformation("Created product {id}", product.Id);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "An error occurred while creating the product");
        }
    }
}
```

### 7. Result Execution

#### Custom Action Result
```csharp
public class CsvResult : IActionResult
{
    private readonly IEnumerable<object> _data;
    private readonly string _fileName;

    public CsvResult(IEnumerable<object> data, string fileName)
    {
        _data = data;
        _fileName = fileName;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.ContentType = "text/csv";
        response.Headers.Add("Content-Disposition", 
            $"attachment; filename={_fileName}");

        using var writer = new StreamWriter(response.Body);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        await csv.WriteRecordsAsync(_data);
    }
}

// Usage in controller
public IActionResult ExportToCsv()
{
    var data = _productService.GetAll();
    return new CsvResult(data, "products.csv");
}
```

### 8. Response Generation

#### Response Pipeline
```csharp
public class ResponseFormattingMiddleware
{
    private readonly RequestDelegate _next;

    public ResponseFormattingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context);

        memoryStream.Position = 0;
        var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

        // Format or transform response if needed
        var formattedResponse = FormatResponse(responseBody);

        var buffer = Encoding.UTF8.GetBytes(formattedResponse);
        using var output = new MemoryStream(buffer);
        await output.CopyToAsync(originalBodyStream);
    }

    private string FormatResponse(string response)
    {
        // Add any response transformation logic here
        return response;
    }
}
```

## Common Scenarios

### 1. Authentication and Authorization
```csharp
[Authorize]
public class SecureController : Controller
{
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    public IActionResult AdminDashboard()
    {
        return View();
    }
}
```

### 2. Error Handling
```csharp
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(
        HttpContext context, Exception exception)
    {
        var result = JsonSerializer.Serialize(new
        {
            Error = "An error occurred while processing your request.",
            Details = exception.Message
        });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return context.Response.WriteAsync(result);
    }
}
```

## Best Practices

1. **Pipeline Configuration**
   - Order middleware correctly
   - Handle errors appropriately
   - Log important information
   - Use async/await consistently

2. **Performance**
   - Minimize middleware usage
   - Use caching effectively
   - Optimize database queries
   - Implement response compression

3. **Security**
   - Validate input early
   - Use HTTPS
   - Implement proper authentication
   - Follow authorization best practices

4. **Maintainability**
   - Follow SOLID principles
   - Use dependency injection
   - Implement proper logging
   - Write clean, testable code

## Related Topics
- [Controller Basics](controller-basics.md)
- [Routing Fundamentals](routing-fundamentals.md)
- [Model Binding](model-binding.md)
- [Error Handling](../../3-Advanced-Concepts/04-Error-Handling/error-handling.md)