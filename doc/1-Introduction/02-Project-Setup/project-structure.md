# ASP.NET Core MVC Project Structure

## Standard Project Layout

```
Solution/
├── src/
│   ├── Application.Web/              # Main MVC Web Application
│   │   ├── Controllers/              # MVC & API Controllers
│   │   ├── Models/                   # View Models & DTOs
│   │   │   ├── ViewModels/          # Specific to views
│   │   │   └── Mapping/             # AutoMapper profiles
│   │   ├── Views/                   # Razor views
│   │   ├── wwwroot/                 # Static files
│   │   └── Areas/                   # Feature areas
│   │
│   ├── Application.Core/            # Core Business Logic
│   │   ├── Entities/               # Domain models
│   │   ├── Interfaces/             # Core interfaces
│   │   ├── Services/               # Business services
│   │   └── Exceptions/             # Custom exceptions
│   │
│   ├── Application.Infrastructure/  # Infrastructure Layer
│   │   ├── Data/                   # Data access
│   │   │   ├── Contexts/          # DbContexts
│   │   │   ├── Repositories/      # Data repositories
│   │   │   └── Migrations/        # EF Migrations
│   │   ├── External/              # External services
│   │   └── Common/                # Shared utilities
│   │
│   └── Application.Shared/         # Shared Components
│       ├── Constants/              # Shared constants
│       ├── Extensions/             # Extension methods
│       └── Helpers/               # Utility helpers
│
├── tests/
│   ├── Application.UnitTests/      # Unit tests
│   ├── Application.IntegrationTests/ # Integration tests
│   └── Application.FunctionalTests/  # End-to-end tests
│
└── tools/                          # Build scripts & tools
```

## Layer Responsibilities

### 1. Presentation Layer (Application.Web)

#### Controllers/
```csharp
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly IMapper _mapper;

    public ProductsController(IProductService productService, IMapper mapper)
    {
        _productService = productService;
        _mapper = mapper;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllAsync();
        var viewModels = _mapper.Map<List<ProductViewModel>>(products);
        return View(viewModels);
    }
}
```

#### Models/ViewModels/
```csharp
public class ProductViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string FormattedPrice => Price.ToString("C");
    public string Category { get; set; }
    public bool IsAvailable { get; set; }
}
```

#### Views/
```cshtml
@model IEnumerable<ProductViewModel>

<div class="container">
    <h2>Products</h2>
    <div class="row">
        @foreach (var product in Model)
        {
            <partial name="_ProductCard" model="product" />
        }
    </div>
</div>
```

### 2. Core Layer (Application.Core)

#### Entities/
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string SKU { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

#### Interfaces/
```csharp
public interface IProductService
{
    Task<Product> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product> CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}
```

#### Services/
```csharp
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Product> CreateAsync(Product product)
    {
        await _repository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return product;
    }
}
```

### 3. Infrastructure Layer (Application.Infrastructure)

#### Data/Contexts/
```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

#### Data/Repositories/
```csharp
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        return await Context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }
}
```

## Best Practices

### 1. Dependency Management
```csharp
// Program.cs
services.AddScoped<IProductRepository, ProductRepository>();
services.AddScoped<IProductService, ProductService>();
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### 2. Configuration Organization
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "AppSettings": {
    "SiteTitle": "My Application",
    "CacheTimeout": 3600
  },
  "Authentication": {
    "JwtSecret": "...",
    "JwtExpiryInDays": 7
  }
}
```

### 3. Middleware Configuration
```csharp
public class Startup
{
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}
```

## Project Organization Guidelines

### 1. Feature Organization
- Group related components by feature
- Use Areas for large feature sets
- Keep controllers thin
- Move business logic to services

### 2. Code Organization
- One class per file
- Clear file naming conventions
- Consistent folder structure
- Proper namespace organization

### 3. Testing Structure
- Mirror source structure in tests
- Separate unit and integration tests
- Use test categories/traits
- Include test helpers and fixtures

## Related Topics
- [Solution Architecture](../../3-Advanced-Concepts/02-Services/solution-architecture.md)
- [Dependency Injection](../../3-Advanced-Concepts/02-Services/dependency-injection.md)
- [Clean Architecture](../../4-Best-Practices/04-Code-Organization/clean-architecture.md)