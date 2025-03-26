# ASP.NET Core MVC and Spring Boot Comparison

## Framework Fundamentals

### Basic Concepts Mapping

| ASP.NET Core MVC | Spring Boot | Notes |
|-----------------|-------------|--------|
| Program.cs | @SpringBootApplication | Application entry and configuration |
| Controllers | @Controller | Handle HTTP requests |
| Models | @Entity | Data representation |
| Views | Thymeleaf templates | UI rendering |
| Services | @Service | Business logic |
| DbContext | JpaRepository | Data access |
| Areas | Modules | Code organization |
| Filters | Interceptors | Cross-cutting concerns |
| Tag Helpers | Thymeleaf dialects | View helpers |

## Code Examples

### 1. Application Entry Point

ASP.NET Core MVC:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
var app = builder.Build();

app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

Spring Boot:
```java
@SpringBootApplication
public class Application {
    public static void main(String[] args) {
        SpringApplication.run(Application.class, args);
    }
}
```

### 2. Controllers

ASP.NET Core MVC:
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> Get(int id)
    {
        var product = await _service.GetByIdAsync(id);
        if (product == null)
            return NotFound();
        return Ok(product);
    }
}
```

Spring Boot:
```java
@RestController
@RequestMapping("/api/products")
public class ProductController {
    private final ProductService service;

    public ProductController(ProductService service) {
        this.service = service;
    }

    @GetMapping("/{id}")
    public ResponseEntity<Product> get(@PathVariable Long id) {
        return service.findById(id)
            .map(ResponseEntity::ok)
            .orElse(ResponseEntity.notFound().build());
    }
}
```

### 3. Models and Validation

ASP.NET Core MVC:
```csharp
public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Display(Name = "In Stock")]
    public bool IsAvailable { get; set; }
}
```

Spring Boot:
```java
@Entity
public class Product {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @NotNull
    @Size(max = 100)
    private String name;

    @Min(0)
    private BigDecimal price;

    @Column(name = "is_available")
    private boolean available;
}
```

### 4. Data Access

ASP.NET Core MVC (Entity Framework):
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
    }
}
```

Spring Boot (JPA):
```java
@Repository
public interface ProductRepository extends JpaRepository<Product, Long> {
    List<Product> findByNameContaining(String name);
    List<Product> findByPriceLessThan(BigDecimal price);
}
```

## Key Differences

### 1. Configuration Approach

ASP.NET Core MVC:
- Code-first configuration in Program.cs
- JSON-based configuration (appsettings.json)
- Options pattern for typed configuration

Spring Boot:
- Annotation-based configuration
- YAML/properties-based configuration
- @ConfigurationProperties for typed configuration

### 2. Dependency Injection

ASP.NET Core MVC:
```csharp
services.AddScoped<IProductService, ProductService>();
services.AddTransient<IEmailService, EmailService>();
services.AddSingleton<ICacheService, CacheService>();
```

Spring Boot:
```java
@Configuration
public class AppConfig {
    @Bean
    @Scope("prototype")
    public EmailService emailService() {
        return new EmailService();
    }
}
```

### 3. View Engine Approaches

ASP.NET Core MVC (Razor):
```cshtml
@model ProductViewModel
<h1>@Model.Name</h1>
<p>Price: @Model.Price.ToString("C")</p>
@if (Model.IsAvailable) {
    <button>Add to Cart</button>
}
```

Spring Boot (Thymeleaf):
```html
<h1 th:text="${product.name}">Product Name</h1>
<p th:text="${#numbers.formatCurrency(product.price)}">$0.00</p>
<button th:if="${product.available}">Add to Cart</button>
```

## Framework Strengths

### ASP.NET Core MVC Advantages
- Strong typing throughout
- Better IDE integration with Visual Studio
- More consistent API design
- Built-in dependency injection
- Better performance in most scenarios

### Spring Boot Advantages
- More mature ecosystem
- Better Java enterprise integration
- More flexible module system
- Rich annotation-based configuration
- Stronger backward compatibility

## Migration Considerations

### From Spring Boot to ASP.NET Core
1. **Controller Migration**
   - Map annotations to attributes
   - Convert dependency injection syntax
   - Adapt routing patterns

2. **View Migration**
   - Convert Thymeleaf to Razor syntax
   - Migrate layout templates
   - Update view components

3. **Data Access Migration**
   - Convert JPA repositories to EF Core
   - Adapt JPQL to LINQ
   - Migrate database configurations

### From ASP.NET Core to Spring Boot
1. **Configuration Changes**
   - Move Program.cs setup to annotations
   - Convert JSON config to YAML/properties
   - Adapt middleware to filters/interceptors

2. **Code Organization**
   - Restructure for Java conventions
   - Convert namespaces to packages
   - Adapt service lifetimes

3. **Testing Approaches**
   - Convert xUnit to JUnit
   - Adapt mock frameworks
   - Update integration tests

## Related Topics
- [Detailed Feature Comparison](../../5-Spring-Comparison/02-Feature-Comparison/README.md)
- [Migration Guide](../../5-Spring-Comparison/03-Migration-Guide/README.md)
- [Enterprise Patterns](../../3-Advanced-Concepts/02-Services/enterprise-patterns.md)