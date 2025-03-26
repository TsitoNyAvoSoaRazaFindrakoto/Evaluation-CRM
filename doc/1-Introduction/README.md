# Introduction to ASP.NET Core MVC

## 1.1 Overview

### What is MVC?
The Model-View-Controller (MVC) pattern separates an application into three main components:
- **Models**: Represent the application's data and business logic
- **Views**: Handle the UI and presentation logic
- **Controllers**: Process incoming requests, interact with models, and determine responses

### Why ASP.NET Core MVC?
ASP.NET Core MVC is ideal for:
- Building large-scale web applications
- Following clean architecture principles
- Implementing RESTful services
- Creating testable and maintainable applications

### Spring Boot Parallels
For developers familiar with Spring Boot:
- ASP.NET Controllers ≈ Spring `@Controller`/`@RestController`
- Model classes ≈ Spring entities/DTOs
- Razor Views ≈ Thymeleaf templates
- Dependency Injection built into both frameworks
- Middleware ≈ Spring Interceptors
- Action filters ≈ Spring AOP

## 1.2 Project Setup

### Creating a New Project

Using .NET CLI:
```bash
dotnet new mvc -n MyWebApp
cd MyWebApp
dotnet run
```

Using Visual Studio:
1. File → New → Project
2. Select "ASP.NET Core Web App (Model-View-Controller)"
3. Configure project name and location

### Project Structure
```
MyWebApp/
├── Controllers/         # Contains MVC controllers
├── Models/             # Data models and view models
├── Views/              # Razor views (.cshtml files)
├── wwwroot/           # Static files (CSS, JS, images)
├── Program.cs         # Application entry point and configuration
└── appsettings.json   # Configuration settings
```

### Key Files
- **Program.cs**: Application startup and service configuration
- **appsettings.json**: Application settings and connection strings
- **_Layout.cshtml**: Main layout template
- **{Controller}/Index.cshtml**: Default view for each controller

### Basic Configuration
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

## 1.3 Getting Started Tips

### Best Practices
1. Follow consistent naming conventions
2. Use dependency injection
3. Keep controllers thin
4. Implement proper error handling
5. Use strongly-typed views

### Common Pitfalls
1. Mixing business logic in controllers
2. Not using view models
3. Hardcoding configuration
4. Ignoring security best practices

### Development Tools
- Visual Studio 2022 or later
- Visual Studio Code with C# extensions
- .NET SDK 8.0 or later