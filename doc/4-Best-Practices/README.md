# Best Practices and Optimization

## 4.1 Performance Optimization

### 4.1.1 Caching Strategies
```csharp
// In-Memory Cache
public class ProductService
{
    private readonly IMemoryCache _cache;
    private const string ProductListCacheKey = "ProductList";

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _cache.GetOrCreateAsync(ProductListCacheKey, async entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
            return await _dbContext.Products.ToListAsync();
        });
    }
}

// Distributed Cache for multi-server environments
public class DistributedCacheService
{
    private readonly IDistributedCache _cache;
    
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration)
    {
        var data = await _cache.GetAsync(key);
        if (data != null)
        {
            return JsonSerializer.Deserialize<T>(data);
        }

        var result = await factory();
        await _cache.SetAsync(
            key,
            JsonSerializer.SerializeToUtf8Bytes(result),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration }
        );
        return result;
    }
}
```

### 4.1.2 Response Compression
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
    });
}
```

### 4.1.3 Async/Await Best Practices
```csharp
// DO: Use ConfigureAwait(false) when no context is needed
public async Task<User> GetUserAsync(int id)
{
    return await _dbContext.Users
        .FindAsync(id)
        .ConfigureAwait(false);
}

// DON'T: Block on async code
// Bad:
public User GetUser(int id)
{
    return GetUserAsync(id).Result; // Can cause deadlocks
}

// DO: Use async all the way
public async Task<IActionResult> UserProfile(int id)
{
    var user = await GetUserAsync(id);
    return View(user);
}
```

## 4.2 Security Best Practices

### 4.2.1 Cross-Site Scripting (XSS) Prevention
```csharp
// In Views: Always HTML encode output
@Html.DisplayFor(model => model.Description)

// For raw HTML (use with caution):
@Html.Raw(HttpUtility.HtmlEncode(model.TrustedHtml))

// Configure CSP headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add(
        "Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline';"
    );
    await next();
});
```

### 4.2.2 CSRF Protection
```csharp
// In Startup.cs
services.AddAntiforgery(options => 
{
    options.HeaderName = "X-CSRF-TOKEN";
});

// In Views:
@inject Microsoft.AspNetCore.Antiforgery.IAntiforgery Antiforgery
@{
    var token = Antiforgery.GetAndStoreTokens(Context).RequestToken;
}
<input type="hidden" name="__RequestVerificationToken" value="@token">

// In Controllers:
[ValidateAntiForgeryToken]
public async Task<IActionResult> Save(ProductModel model)
```

### 4.2.3 Secure Configuration
```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Use User Secrets in Development
        if (_env.IsDevelopment())
        {
            builder.AddUserSecrets<Startup>();
        }
        else
        {
            // Use Azure Key Vault or similar in Production
            builder.AddAzureKeyVault(
                new Uri($"https://{builder.Configuration["KeyVault:Name"]}.vault.azure.net/"),
                new DefaultAzureCredential());
        }
    }
}
```

## 4.3 Testing Strategies

### 4.3.1 Unit Testing Controllers
```csharp
public class ProductControllerTests
{
    [Fact]
    public async Task GetProduct_ReturnsNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var mockService = new Mock<IProductService>();
        mockService.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Product)null);
        
        var controller = new ProductController(mockService.Object);

        // Act
        var result = await controller.Get(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
```

### 4.3.2 Integration Testing
```csharp
public class ProductApiTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;

    public ProductApiTests(WebApplicationFactory<Startup> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProducts_ReturnsSuccessStatusCode()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
    }
}
```

## 4.4 Code Organization

### 4.4.1 Solution Structure
```
MyApp/
├── src/
│   ├── MyApp.Web/           # MVC Application
│   ├── MyApp.Core/          # Domain Models, Interfaces
│   ├── MyApp.Infrastructure/# Data Access, External Services
│   └── MyApp.Services/      # Business Logic
├── tests/
│   ├── MyApp.UnitTests/
│   └── MyApp.IntegrationTests/
└── tools/                   # Scripts, Tools
```

### 4.4.2 SOLID Principles
```csharp
// Single Responsibility Principle
public class OrderProcessor
{
    private readonly IOrderValidator _validator;
    private readonly IOrderRepository _repository;
    private readonly IEmailService _emailService;

    public OrderProcessor(
        IOrderValidator validator,
        IOrderRepository repository,
        IEmailService emailService)
    {
        _validator = validator;
        _repository = repository;
        _emailService = emailService;
    }

    public async Task ProcessOrderAsync(Order order)
    {
        await _validator.ValidateAsync(order);
        await _repository.SaveAsync(order);
        await _emailService.SendOrderConfirmationAsync(order);
    }
}

// Interface Segregation Principle
public interface IOrderReader
{
    Task<Order> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetAllAsync();
}

public interface IOrderWriter
{
    Task SaveAsync(Order order);
    Task DeleteAsync(int id);
}

// Classes can implement one or both interfaces as needed
public class OrderRepository : IOrderReader, IOrderWriter
{
    // Implementation
}
```