# Development Tools for ASP.NET Core MVC

## Integrated Development Environments (IDEs)

### Visual Studio 2022
#### Essential Features
- Built-in ASP.NET Core templates
- Intelligent code completion
- Integrated debugging
- Package management
- Git integration
- Built-in testing tools

#### Recommended Extensions
```plaintext
- Web Essentials
- CodeMaid
- ReSharper (3rd party)
- Entity Framework Power Tools
- Azure Tools
```

#### Configuration
```json
// .editorconfig
root = true

[*]
indent_style = space
indent_size = 4
end_of_line = crlf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

[*.{json,yml,yaml,xml}]
indent_size = 2

[*.cs]
csharp_style_var_for_built_in_types = true
csharp_style_var_when_type_is_apparent = true
csharp_style_var_elsewhere = true
```

### Visual Studio Code
#### Required Extensions
```json
{
    "recommendations": [
        "ms-dotnettools.csharp",
        "ms-dotnettools.dotnet-interactive-vscode",
        "formulahendry.dotnet-test-explorer",
        "kreativ-software.csharpextensions",
        "jmrog.vscode-nuget-package-manager",
        "fernandoescolar.vscode-solution-explorer"
    ]
}
```

#### Workspace Settings
```json
{
    "files.exclude": {
        "**/bin": true,
        "**/obj": true
    },
    "omnisharp.enableRoslynAnalyzers": true,
    "omnisharp.enableEditorConfigSupport": true,
    "dotnet-test-explorer.testProjectPath": "**/*Tests.csproj"
}
```

## Command Line Tools

### .NET CLI
#### Essential Commands
```bash
# Project commands
dotnet new mvc                     # Create new MVC project
dotnet run                        # Run the application
dotnet watch run                  # Run with hot reload
dotnet build                      # Build the project
dotnet test                       # Run tests
dotnet add package PackageName    # Add NuGet package
dotnet ef migrations add Name     # Add EF Core migration
dotnet ef database update        # Update database

# Development certificates
dotnet dev-certs https --clean
dotnet dev-certs https --trust

# Publishing
dotnet publish -c Release
```

### Entity Framework Core Tools
```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef

# Common commands
dotnet ef dbcontext info
dotnet ef migrations list
dotnet ef dbcontext scaffold "Connection" Microsoft.EntityFrameworkCore.SqlServer
```

## Browser Development Tools

### Browser Extensions
- Chrome DevTools
- Redux DevTools
- Angular DevTools
- Browser Router

### Debug Configuration
```json
// launch.json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch (web)",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/bin/Debug/net8.0/MyApp.dll",
            "args": [],
            "cwd": "${workspaceFolder}",
            "stopAtEntry": false,
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            }
        }
    ]
}
```

## Testing Tools

### xUnit Test Runner
```json
// settings.json
{
    "dotnet-test-explorer.testProjectPath": "**/*Tests.csproj",
    "dotnet-test-explorer.autoWatch": true
}
```

### Mock Framework Setup
```xml
<!-- Test project .csproj -->
<ItemGroup>
    <PackageReference Include="Moq" Version="4.18.0" />
    <PackageReference Include="xunit" Version="2.4.1" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.0.0" />
</ItemGroup>
```

## Database Tools

### SQL Server Management Studio
- Database management
- Query execution
- Performance tuning
- Backup and restore

### Azure Data Studio
```json
{
    "mssql.connections": [
        {
            "server": "(localdb)\\MSSQLLocalDB",
            "database": "MyAppDb",
            "authenticationType": "Integrated",
            "profileName": "Local Development"
        }
    ]
}
```

## Containerization Tools

### Docker Integration
```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyApp.csproj", "./"]
RUN dotnet restore "MyApp.csproj"
COPY . .
RUN dotnet build "MyApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

### Docker Compose
```yaml
# docker-compose.yml
version: '3.8'
services:
  webapp:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    depends_on:
      - db

  db:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      - SA_PASSWORD=YourStrong@Passw0rd
      - ACCEPT_EULA=Y
    ports:
      - "1433:1433"
```

## CI/CD Tools

### GitHub Actions
```yaml
# .github/workflows/build.yml
name: .NET Build and Test

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 8.0.x
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build
```

## Performance Tools

### Application Insights
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-key-here",
    "EnableAdaptiveSampling": true,
    "EnablePerformanceCounterCollectionModule": true
  }
}
```

## Related Topics
- [Visual Studio Tips and Tricks](../../4-Best-Practices/04-Code-Organization/vs-tips.md)
- [Debugging Guide](../../3-Advanced-Concepts/04-Error-Handling/debugging.md)
- [Continuous Integration](../../4-Best-Practices/04-Code-Organization/ci-cd.md)