# ASP.NET Core MVC Configuration

## Configuration Sources

### 1. JSON Configuration Files
```json
// appsettings.json
{
  "AppSettings": {
    "SiteName": "My Application",
    "SupportEmail": "support@example.com"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}

// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  }
}
```

### 2. Environment Variables
```bash
# Development environment variables
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection="Server=..."
AppSettings__SiteName="Dev Application"
```

### 3. User Secrets (Development)
```json
// secrets.json
{
  "Authentication:ApiKey": "your-secret-key",
  "SmtpSettings:Password": "email-password"
}
```

## Configuration Setup

### 1. Basic Configuration
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add configuration sources
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}
```

### 2. Strongly Typed Configuration
```csharp
// Configuration class
public class AppSettings
{
    public string SiteName { get; set; }
    public string SupportEmail { get; set; }
    public bool EnableNotifications { get; set; }
}

// Registration
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));

// Usage in services/controllers
public class EmailService
{
    private readonly AppSettings _settings;

    public EmailService(IOptions<AppSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendSupportEmail(string message)
    {
        // Use _settings.SupportEmail
    }
}
```

## Environment-Specific Configuration

### 1. Environment Setup
```csharp
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configure based on environment
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddDevelopmentServices();
        }
        else
        {
            builder.Services.AddProductionServices();
        }
    }
}
```

### 2. Environment-Specific Services
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDevelopmentServices(
        this IServiceCollection services)
    {
        services.AddTransient<IEmailService, FakeEmailService>();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("DevDb"));
        return services;
    }

    public static IServiceCollection AddProductionServices(
        this IServiceCollection services)
    {
        services.AddTransient<IEmailService, SmtpEmailService>();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        return services;
    }
}
```

## Advanced Configuration

### 1. Custom Configuration Provider
```csharp
public class DatabaseConfigurationProvider : ConfigurationProvider
{
    private readonly Action<DbContextOptionsBuilder> _options;

    public DatabaseConfigurationProvider(
        Action<DbContextOptionsBuilder> options)
    {
        _options = options;
    }

    public override void Load()
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        _options(builder);

        using (var context = new ApplicationDbContext(builder.Options))
        {
            var settings = context.Settings.ToDictionary(s => s.Key, s => s.Value);
            Data = settings;
        }
    }
}
```

### 2. Configuration Validation
```csharp
public class SmtpSettings
{
    [Required]
    public string Server { get; set; }

    [Required]
    [Range(1, 65535)]
    public int Port { get; set; }

    [Required]
    [EmailAddress]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }

    public bool EnableSsl { get; set; } = true;
}

// Validation
services.AddOptions<SmtpSettings>()
    .Bind(configuration.GetSection("SmtpSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### 3. Reloadable Configuration
```csharp
public class EmailService
{
    private readonly IOptionsMonitor<SmtpSettings> _settings;

    public EmailService(IOptionsMonitor<SmtpSettings> settings)
    {
        _settings = settings;
        // Monitor configuration changes
        _settings.OnChange(settings => 
        {
            // Reinitialize email client with new settings
        });
    }
}
```

## Security Best Practices

### 1. Secrets Management
- Use User Secrets in development
- Use Azure Key Vault in production
- Never commit sensitive data to source control

### 2. Configuration Encryption
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(@"path\to\keys"))
        .ProtectKeysWithCertificate(
            new X509Certificate2("certificate.pfx"));
}
```

### 3. Secure Configuration Access
```csharp
public class SecureConfigurationProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecureConfigurationProvider> _logger;

    public SecureConfigurationProvider(
        IConfiguration configuration,
        ILogger<SecureConfigurationProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GetSecureValue(string key)
    {
        try
        {
            var value = _configuration[key];
            if (string.IsNullOrEmpty(value))
            {
                _logger.LogWarning($"Configuration key {key} not found");
                return null;
            }
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error accessing configuration key {key}");
            throw;
        }
    }
}
```

## Related Topics
- [Environment Setup](environment-setup.md)
- [Security Best Practices](../../4-Best-Practices/02-Security/security-configuration.md)
- [Azure Key Vault Integration](../../3-Advanced-Concepts/05-API-Integration/azure-key-vault.md)