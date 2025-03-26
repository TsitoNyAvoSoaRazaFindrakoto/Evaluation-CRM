# Controller Security

## Authentication & Authorization
```csharp
[Authorize]
public class AdminController : Controller
{
    [Authorize(Roles = "Admin")]
    public IActionResult SensitiveAction()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult PublicAction()
    {
        return View();
    }
}
```

## Anti-Forgery Tokens
```csharp
[ValidateAntiForgeryToken]
[HttpPost]
public IActionResult Create(ProductViewModel model)
{
    // Implementation
}
```

## Input Validation
```csharp
public IActionResult ProcessInput([FromBody] InputModel model)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    // Sanitize input
    var sanitizedInput = _sanitizer.Sanitize(model.Input);
    
    // Process sanitized input
    return Ok();
}
```

## Security Headers
```csharp
public class SecurityHeadersAttribute : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        var headers = context.HttpContext.Response.Headers;
        
        headers.Add("X-Frame-Options", "DENY");
        headers.Add("X-XSS-Protection", "1; mode=block");
        headers.Add("X-Content-Type-Options", "nosniff");
        headers.Add("Referrer-Policy", "no-referrer");
        headers.Add("Content-Security-Policy", "default-src 'self'");
        
        base.OnResultExecuting(context);
    }
}
```

## Rate Limiting
```csharp
[EnableRateLimiting("fixed")]
public class ApiController : ControllerBase
{
    [EnableRateLimiting("sliding")]
    public IActionResult RateLimitedEndpoint()
    {
        return Ok();
    }
}
```
