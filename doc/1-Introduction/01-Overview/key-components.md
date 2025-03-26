# Key Components of ASP.NET Core MVC

## Core Framework Components

### Program.cs
The application entry point and configuration hub.
```csharp
var builder = WebApplication.CreateBuilder(args);

// Service configuration
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>();

var app = builder.Build();

// Middleware configuration
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

### Middleware Pipeline
Components that handle requests and responses:
- Authentication
- Static Files
- Routing
- Exception Handling
- Custom Middleware

## Application Components

### Models
- Domain Models
- View Models
- Data Transfer Objects (DTOs)
```csharp
// Domain Model
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// View Model
public class CustomerViewModel
{
    public string Name { get; set; }
    public string FormattedEmail { get; set; }
    public List<OrderSummary> RecentOrders { get; set; }
}
```

### Controllers
Handle user interactions and application flow:
- Action Methods
- Filters
- Model Binding
- Action Results

```csharp
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetCustomer(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
            return NotFound();
            
        return Ok(customer);
    }
}
```

### Views
Presentation layer components:
- Razor Views
- Partial Views
- View Components
- Tag Helpers
- HTML Helpers

```cshtml
@model CustomerViewModel

<div class="customer-profile">
    <h2>@Model.Name</h2>
    <email-display address="@Model.FormattedEmail"></email-display>
    
    <partial name="_RecentOrders" model="@Model.RecentOrders" />
    
    @await Component.InvokeAsync("CustomerStats", new { customerId = Model.Id })
</div>
```

## Infrastructure Components

### Services
Business logic and data access:
```csharp
public interface ICustomerService
{
    Task<Customer> GetByIdAsync(int id);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer> CreateAsync(Customer customer);
}

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;

    public CustomerService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Implementation
}
```

### Data Access
Entity Framework Core and other data providers:
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>()
            .HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId);
    }
}
```

## Supporting Components

### Configuration
Multiple configuration sources:
- appsettings.json
- Environment Variables
- User Secrets
- Azure Key Vault

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Dependency Injection
Built-in IoC container:
```csharp
services.AddScoped<ICustomerService, CustomerService>();
services.AddTransient<IEmailService, EmailService>();
services.AddSingleton<ICacheService, CacheService>();
```

### Logging
Structured logging support:
```csharp
public class CustomerController : Controller
{
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ILogger<CustomerController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Displaying customer list");
        return View();
    }
}
```

## Related Topics
- [Architecture Overview](architecture.md)
- [Project Structure](../02-Project-Setup/project-structure.md)
- [Dependency Injection](../../3-Advanced-Concepts/02-Services/dependency-injection.md)