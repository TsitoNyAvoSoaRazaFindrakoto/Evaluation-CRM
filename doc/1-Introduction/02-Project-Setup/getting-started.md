# Getting Started with ASP.NET Core MVC

## Prerequisites

### Required Software
1. **.NET SDK 8.0 or later**
   - Download from: [.NET Download Page](https://dotnet.microsoft.com/download)
   - Verify installation:
     ```bash
     dotnet --version
     ```

2. **Development IDE**
   - Visual Studio 2022 or later (recommended for Windows)
   - Visual Studio Code with C# extensions (cross-platform)
   - JetBrains Rider (cross-platform)

3. **Database Tools**
   - SQL Server Management Studio (for SQL Server)
   - Or Azure Data Studio (cross-platform)

## Creating a New Project

### Using .NET CLI
```bash
# Create a new MVC project
dotnet new mvc -n MyProjectName

# Navigate to project directory
cd MyProjectName

# Run the project
dotnet run
```

### Using Visual Studio
1. File → New → Project
2. Select "ASP.NET Core Web App (Model-View-Controller)"
3. Configure project settings:
   - Project name
   - Location
   - Solution name
   - Framework version
4. Click Create

## Initial Project Structure
```
MyProjectName/
├── Controllers/
│   └── HomeController.cs
├── Models/
│   └── ErrorViewModel.cs
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml
│   │   └── Privacy.cshtml
│   └── Shared/
│       ├── _Layout.cshtml
│       └── Error.cshtml
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/
├── Program.cs
├── appsettings.json
└── MyProjectName.csproj
```

## Essential Configuration

### 1. Database Connection
In appsettings.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyProjectName;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

In Program.cs:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
```

### 2. Authentication Setup
```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "oidc";
});
```

### 3. Static Files and Routing
```csharp
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

## Adding Essential Packages

### Basic Development
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
```

### Authentication and Authorization
```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### Development Tools
```bash
dotnet add package Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation
```

## Initial Development Tasks

### 1. Create Database Context
```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Add your model configurations here
    }
}
```

### 2. Run Initial Migration
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3. Create First Controller
```csharp
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
}
```

### 4. Add First View
```cshtml
@{
    ViewData["Title"] = "Home Page";
}

<div class="text-center">
    <h1>Welcome to My Project</h1>
    <p>Learn about building web apps with ASP.NET Core MVC.</p>
</div>
```

## Development Workflow

### 1. Running the Application
```bash
# Run in development mode
dotnet run

# Run and watch for file changes
dotnet watch run
```

### 2. Common Development Tasks
- Create new controller: `dotnet new controller -n ProductsController`
- Create new view: Create .cshtml file in Views/{Controller}/
- Add new model: Create class in Models/ directory
- Update database: `dotnet ef database update`

### 3. Building for Production
```bash
# Publish the application
dotnet publish -c Release

# Run in production mode
dotnet run --environment Production
```

## Next Steps
1. [Configure Project Structure](project-structure.md)
2. [Set Up Authentication](../../3-Advanced-Concepts/03-Authentication/setup.md)
3. [Add Database Access](../../2-Core-Components/01-Models/database-setup.md)
4. [Create Your First CRUD Controller](../../2-Core-Components/02-Controllers/crud-operations.md)