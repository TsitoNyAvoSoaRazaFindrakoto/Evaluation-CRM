# Common Development Tasks in ASP.NET Core MVC

## Entity Framework Operations

### Database Management
```bash
# Add a new migration
dotnet ef migrations add AddProductTable

# Remove last migration
dotnet ef migrations remove

# Update database
dotnet ef database update

# Generate SQL script
dotnet ef migrations script
```

### Model Updates
```csharp
// 1. Update model
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }  // New field
}

// 2. Add migration
// dotnet ef migrations add AddProductDescription

// 3. Update database
// dotnet ef database update
```

## Package Management

### Common NuGet Operations
```bash
# Add packages
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.AspNetCore.Identity.UI
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection

# Update packages
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0

# List outdated packages
dotnet list package --outdated

# Remove package
dotnet remove package PackageName
```

## Authentication Setup

### Cookie Authentication
```csharp
// Program.cs
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Apply authentication
app.UseAuthentication();
app.UseAuthorization();
```

### Identity Setup
```csharp
// Program.cs
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
});
```

## Adding New Features

### Create New Controller
```csharp
// Controllers/ProductsController.cs
public class ProductsController : Controller
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllAsync();
        return View(products);
    }
}
```

### Add Views
```bash
# Create views folder
mkdir Views/Products

# Create view files
touch Views/Products/Index.cshtml
touch Views/Products/_ProductCard.cshtml
```

### Create View Content
```cshtml
@* Views/Products/Index.cshtml *@
@model IEnumerable<ProductViewModel>

<div class="container">
    <h2>Products</h2>
    <a asp-action="Create" class="btn btn-primary mb-3">Add New Product</a>

    <div class="row">
        @foreach (var product in Model)
        {
            <partial name="_ProductCard" model="product" />
        }
    </div>
</div>
```

## Static Files Management

### Configure Static Files
```csharp
// Program.cs
app.UseStaticFiles();

// Optional: Configure static file options
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "MyStaticFiles")),
    RequestPath = "/StaticFiles"
});
```

### Add Client-Side Libraries
```bash
# Using LibMan
libman init
libman install bootstrap
libman install jquery
```

## Logging Configuration

### Setup Logging
```csharp
// Program.cs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventLog();

// Optional: Configure logging levels
builder.Logging.SetMinimumLevel(LogLevel.Information);
```

### Add Logging to Components
```csharp
public class ProductService
{
    private readonly ILogger<ProductService> _logger;

    public ProductService(ILogger<ProductService> logger)
    {
        _logger = logger;
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving product with ID: {Id}", id);
        // Implementation
    }
}
```

## Error Handling

### Global Error Handling
```csharp
// Program.cs
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
}
```

### Custom Error Pages
```cshtml
@* Views/Shared/Error.cshtml *@
@model ErrorViewModel

<div class="text-center">
    <h1 class="text-danger">Error.</h1>
    <h2 class="text-danger">An error occurred while processing your request.</h2>

    @if (Model.ShowRequestId)
    {
        <p>Request ID: <code>@Model.RequestId</code></p>
    }
</div>
```

## Performance Optimization

### Response Caching
```csharp
// Enable response caching
builder.Services.AddResponseCaching();

// Controller usage
[ResponseCache(Duration = 60)]
public IActionResult Index()
{
    return View();
}
```

### Memory Cache
```csharp
public class CacheService
{
    private readonly IMemoryCache _cache;

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory)
    {
        return await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await factory();
        });
    }
}
```

## Related Topics
- [Database Management](../../2-Core-Components/01-Models/database-management.md)
- [Authentication Guide](../../3-Advanced-Concepts/03-Authentication/README.md)
- [Performance Tips](../../4-Best-Practices/01-Performance/caching-strategies.md)