# Advanced Concepts in ASP.NET Core MVC

## 3.1 Routing

### 3.1.1 Conventional Routing
```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Custom route patterns
app.MapControllerRoute(
    name: "blog",
    pattern: "blog/{year}/{month}/{day}/{slug}",
    defaults: new { controller = "Blog", action = "Post" });
```

### 3.1.2 Attribute Routing
```csharp
[Route("api/[controller]")]
public class ProductsController : Controller
{
    [HttpGet("")]  // GET: api/products
    public IActionResult GetAll() { }

    [HttpGet("{id}")]  // GET: api/products/5
    public IActionResult GetById(int id) { }

    [HttpGet("category/{category}")]  // GET: api/products/category/electronics
    public IActionResult GetByCategory(string category) { }
}
```

## 3.2 Services and Dependency Injection

### 3.2.1 Service Registration
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Transient: New instance for every request
    services.AddTransient<IEmailService, EmailService>();

    // Scoped: Same instance within a request
    services.AddScoped<IUserService, UserService>();

    // Singleton: Same instance for the application lifetime
    services.AddSingleton<ICacheService, CacheService>();
}
```

### 3.2.2 Custom Middleware
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
        _logger.LogInformation($"Handling request: {context.Request.Path}");
        await _next(context);
        _logger.LogInformation($"Finished handling request");
    }
}

// Extension method
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
```

## 3.3 Authentication and Authorization

### 3.3.1 Authentication Setup
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Configuration["Jwt:Issuer"],
            ValidAudience = Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
        };
    });
}
```

### 3.3.2 Authorization Policies
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole("Admin"));

        options.AddPolicy("RequireMFA", policy =>
            policy.RequireClaim("mfa-enabled", "true"));
    });
}

[Authorize(Policy = "AdminOnly")]
public class AdminController : Controller
{
    // Only accessible by admins
}
```

## 3.4 Error Handling

### 3.4.1 Global Exception Handler
```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing your request.",
            Detail = exception.Message
        };

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
```

### 3.4.2 Custom Error Pages
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (!env.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseStatusCodePagesWithReExecute("/Error/{0}");
    }
}
```

## 3.5 API Integration

### 3.5.1 HttpClient Factory
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHttpClient("weatherapi", client =>
    {
        client.BaseAddress = new Uri("https://api.weather.com/");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    })
    .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(
        3, // Number of retries
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) // Exponential backoff
    ));
}

public class WeatherService
{
    private readonly IHttpClientFactory _clientFactory;

    public WeatherService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<WeatherForecast> GetForecastAsync(string city)
    {
        var client = _clientFactory.CreateClient("weatherapi");
        return await client.GetFromJsonAsync<WeatherForecast>($"forecast/{city}");
    }
}
```

### 3.5.2 Background Services
```csharp
public class QueuedHostedService : BackgroundService
{
    private readonly ILogger<QueuedHostedService> _logger;
    private readonly IBackgroundTaskQueue _taskQueue;

    public QueuedHostedService(
        IBackgroundTaskQueue taskQueue,
        ILogger<QueuedHostedService> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _taskQueue.DequeueAsync(stoppingToken);

            try
            {
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing task work item.");
            }
        }
    }
}
```