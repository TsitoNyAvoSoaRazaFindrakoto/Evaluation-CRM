# State Management in ASP.NET Core MVC

## Overview

HTTP is a stateless protocol, but web applications often need to maintain state across requests. ASP.NET Core MVC provides several mechanisms for state management.

## Session State

### Configuration
```csharp
// Program.cs
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Enable session middleware
app.UseSession();
```

### Using Session
```csharp
public class CartController : Controller
{
    // Store in session
    public IActionResult AddToCart(int productId)
    {
        var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
        cart.Add(new CartItem { ProductId = productId });
        HttpContext.Session.Set("Cart", cart);
        return RedirectToAction("Index");
    }

    // Retrieve from session
    public IActionResult ViewCart()
    {
        var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
        return View(cart);
    }
}

// Session extension methods
public static class SessionExtensions
{
    public static void Set<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}
```

## Cookies

### Basic Cookie Usage
```csharp
public class PreferencesController : Controller
{
    // Set cookie
    public IActionResult SetTheme(string theme)
    {
        var options = new CookieOptions
        {
            Expires = DateTime.Now.AddYears(1),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        Response.Cookies.Append("theme", theme, options);
        return RedirectToAction("Index", "Home");
    }

    // Read cookie
    public IActionResult GetTheme()
    {
        var theme = Request.Cookies["theme"] ?? "light";
        return Json(new { theme });
    }

    // Delete cookie
    public IActionResult ResetTheme()
    {
        Response.Cookies.Delete("theme");
        return RedirectToAction("Index", "Home");
    }
}
```

### Secure Cookie Handling
```csharp
public class SecureCookieManager
{
    private readonly IDataProtector _protector;

    public SecureCookieManager(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("Cookies.Auth");
    }

    public void SetSecureCookie(HttpResponse response, string key, string value)
    {
        var encryptedValue = _protector.Protect(value);
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(30)
        };

        response.Cookies.Append(key, encryptedValue, options);
    }

    public string GetSecureCookie(HttpRequest request, string key)
    {
        var encryptedValue = request.Cookies[key];
        if (string.IsNullOrEmpty(encryptedValue))
            return null;

        try
        {
            return _protector.Unprotect(encryptedValue);
        }
        catch
        {
            return null;
        }
    }
}
```

## TempData

### Basic TempData Usage
```csharp
public class AccountController : Controller
{
    public IActionResult Login(LoginModel model)
    {
        if (ModelState.IsValid)
        {
            // Process login
            TempData["Message"] = "Welcome back!";
            return RedirectToAction("Dashboard");
        }
        return View(model);
    }

    public IActionResult Dashboard()
    {
        // TempData["Message"] will be available here
        return View();
    }
}
```

### TempData Provider
```csharp
public class CustomTempDataProvider : ITempDataProvider
{
    private readonly IDistributedCache _cache;
    private readonly IDataProtector _protector;

    public CustomTempDataProvider(
        IDistributedCache cache,
        IDataProtectionProvider protectionProvider)
    {
        _cache = cache;
        _protector = protectionProvider.CreateProtector("TempData");
    }

    public IDictionary<string, object> LoadTempData(HttpContext context)
    {
        var key = GetKey(context);
        var data = _cache.GetString(key);

        if (string.IsNullOrEmpty(data))
            return new Dictionary<string, object>();

        try
        {
            var unprotectedData = _protector.Unprotect(data);
            return JsonSerializer.Deserialize<Dictionary<string, object>>(unprotectedData);
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SaveTempData(HttpContext context, IDictionary<string, object> values)
    {
        var key = GetKey(context);
        if (values == null || !values.Any())
        {
            _cache.Remove(key);
            return;
        }

        var data = JsonSerializer.Serialize(values);
        var protectedData = _protector.Protect(data);
        _cache.SetString(key, protectedData, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20)
        });
    }

    private string GetKey(HttpContext context)
    {
        return $"TempData_{context.User.Identity?.Name ?? "anonymous"}";
    }
}
```

## Cache

### In-Memory Cache
```csharp
public class ProductService
{
    private readonly IMemoryCache _cache;
    private readonly IProductRepository _repository;

    public ProductService(IMemoryCache cache, IProductRepository repository)
    {
        _cache = cache;
        _repository = repository;
    }

    public async Task<Product> GetProductAsync(int id)
    {
        var cacheKey = $"product_{id}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromHours(1));
            entry.SetSlidingExpiration(TimeSpan.FromMinutes(10));
            return await _repository.GetByIdAsync(id);
        });
    }
}
```

### Distributed Cache
```csharp
public class DistributedCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<DistributedCacheService> _logger;

    public DistributedCacheService(
        IDistributedCache cache,
        ILogger<DistributedCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory)
    {
        var json = await _cache.GetStringAsync(key);
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing cached value");
            }
        }

        var value = await factory();
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };

        await _cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(value),
            options);

        return value;
    }
}
```

## Best Practices

1. **Choose the Right Storage Method**
   - Session: User-specific temporary data
   - Cookies: Small, client-side data
   - TempData: Cross-request messages
   - Cache: Application-wide data

2. **Security Considerations**
   - Encrypt sensitive data
   - Use secure cookie options
   - Implement proper timeout policies

3. **Performance Optimization**
   - Cache frequently accessed data
   - Use distributed cache for scalability
   - Monitor memory usage

4. **Data Size Management**
   - Keep session data minimal
   - Use appropriate expiration times
   - Clean up unused data

## Common Scenarios

### Shopping Cart
```csharp
public class ShoppingCartService
{
    private readonly ISession _session;
    private const string CartKey = "ShoppingCart";

    public ShoppingCartService(IHttpContextAccessor httpContextAccessor)
    {
        _session = httpContextAccessor.HttpContext.Session;
    }

    public void AddItem(CartItem item)
    {
        var cart = GetCart();
        var existingItem = cart.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            cart.Add(item);
        }
        SaveCart(cart);
    }

    private List<CartItem> GetCart()
    {
        return _session.Get<List<CartItem>>(CartKey) ?? new List<CartItem>();
    }

    private void SaveCart(List<CartItem> cart)
    {
        _session.Set(CartKey, cart);
    }
}
```

### User Preferences
```csharp
public class UserPreferencesService
{
    private readonly IDistributedCache _cache;
    private readonly string _prefix = "UserPrefs_";

    public UserPreferencesService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task SavePreferencesAsync(string userId, UserPreferences prefs)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365)
        };

        await _cache.SetStringAsync(
            $"{_prefix}{userId}",
            JsonSerializer.Serialize(prefs),
            options);
    }

    public async Task<UserPreferences> GetPreferencesAsync(string userId)
    {
        var json = await _cache.GetStringAsync($"{_prefix}{userId}");
        return string.IsNullOrEmpty(json) 
            ? new UserPreferences()
            : JsonSerializer.Deserialize<UserPreferences>(json);
    }
}
```

## Related Topics
- [Security Best Practices](../../4-Best-Practices/02-Security/security-guidelines.md)
- [Caching Strategies](../../4-Best-Practices/01-Performance/caching-strategies.md)
- [Session Management](../../3-Advanced-Concepts/03-Authentication/session-management.md)