# State Management in ASP.NET MVC

State management involves maintaining data across requests in a web application.

## Session State

```csharp
// Controller
public class HomeController : Controller
{
    public IActionResult Index()
    {
        HttpContext.Session.SetString("UserName", "John");
        HttpContext.Session.SetInt32("UserId", 123);
    }
}
```

## TempData

```csharp
public IActionResult Process()
{
    TempData["Message"] = "Operation successful";
    return RedirectToAction("Index");
}
```

## Cookies

```csharp
// Setting cookies
Response.Cookies.Append("UserPreference", "darkMode", new CookieOptions
{
    Expires = DateTime.Now.AddDays(30)
});

// Reading cookies
string preference = Request.Cookies["UserPreference"];
```

## Cache

```csharp
// Using IMemoryCache
public class HomeController : Controller
{
    private readonly IMemoryCache _cache;

    public HomeController(IMemoryCache cache)
    {
        _cache = cache;
    }

    public IActionResult Index()
    {
        _cache.Set("key", "value", TimeSpan.FromMinutes(30));
    }
}
```
