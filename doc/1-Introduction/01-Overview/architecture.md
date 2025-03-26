# ASP.NET Core MVC Architecture

## High-Level Architecture

```plaintext
┌─────────────────────────────────────────────┐
│               Client Browser                │
└───────────────────────┬─────────────────────┘
                        ↓
┌─────────────────────────────────────────────┐
│              ASP.NET Core Host              │
│  ┌─────────────────────────────────────┐    │
│  │         Middleware Pipeline         │    │
│  └─────────────────────────────────────┘    │
│  ┌─────────────────────────────────────┐    │
│  │             MVC Framework           │    │
│  │  ┌─────────┐  ┌─────────────────┐  │    │
│  │  │ Routing │→ │   Controllers   │  │    │
│  │  └─────────┘  └────────┬────────┘  │    │
│  │                        ↓            │    │
│  │  ┌─────────┐     ┌─────────┐       │    │
│  │  │ Models  │←──→ │Services │       │    │
│  │  └─────────┘     └─────────┘       │    │
│  │        ↑             ↑             │    │
│  │        │         ┌───────┐         │    │
│  │        └────────→│ Views │         │    │
│  │                  └───────┘         │    │
│  └─────────────────────────────────────┘    │
└─────────────────────────────────────────────┘
```

## Request Processing Pipeline

1. **HTTP Request Entry**
   - Request arrives at the application
   - Middleware components process the request

2. **Routing**
   ```csharp
   app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Index}/{id?}");
   ```

3. **Controller Processing**
   - Action method selection
   - Model binding
   - Action filters
   - Action execution

4. **Service Layer**
   - Business logic processing
   - Data access coordination
   - External service integration

5. **View Rendering**
   - Model data preparation
   - View selection
   - View component processing
   - HTML generation

## Architectural Layers

### 1. Presentation Layer
- Controllers and Views
- UI Components
- Client-side assets

### 2. Business Layer
```csharp
public interface IOrderProcessor
{
    Task<Order> ProcessOrderAsync(OrderRequest request);
}

public class OrderProcessor : IOrderProcessor
{
    private readonly IOrderValidator _validator;
    private readonly IOrderRepository _repository;
    private readonly IPaymentService _paymentService;

    public OrderProcessor(
        IOrderValidator validator,
        IOrderRepository repository,
        IPaymentService paymentService)
    {
        _validator = validator;
        _repository = repository;
        _paymentService = paymentService;
    }

    public async Task<Order> ProcessOrderAsync(OrderRequest request)
    {
        await _validator.ValidateAsync(request);
        var payment = await _paymentService.ProcessPaymentAsync(request.Payment);
        var order = await _repository.CreateAsync(request.ToOrder());
        return order;
    }
}
```

### 3. Data Access Layer
```csharp
public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order);
    Task<Order> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetByCustomerAsync(int customerId);
}

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Implementation
}
```

## Cross-Cutting Concerns

### 1. Authentication & Authorization
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        // JWT configuration
    });

services.AddAuthorization(options => {
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});
```

### 2. Logging & Monitoring
```csharp
services.AddLogging(builder => {
    builder.AddConsole();
    builder.AddDebug();
    builder.AddApplicationInsights();
});
```

### 3. Caching
```csharp
services.AddStackExchangeRedisCache(options => {
    options.Configuration = Configuration.GetConnectionString("Redis");
});
```

### 4. Error Handling
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
}
```

## Design Patterns Used

1. **MVC Pattern**
   - Separation of concerns
   - Maintainable codebase
   - Testable components

2. **Repository Pattern**
   - Data access abstraction
   - Swappable implementations
   - Testability improvement

3. **Unit of Work**
   - Transaction management
   - Data consistency
   - Atomic operations

4. **Factory Pattern**
   - Object creation abstraction
   - Implementation flexibility
   - Dependency management

## Related Topics
- [Project Structure](../02-Project-Setup/project-structure.md)
- [Dependency Injection](../../3-Advanced-Concepts/02-Services/dependency-injection.md)
- [Service Lifetimes](../../3-Advanced-Concepts/02-Services/service-lifetimes.md)