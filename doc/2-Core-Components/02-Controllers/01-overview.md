# ASP.NET MVC Controllers Overview

## Introduction
Controllers are fundamental components in ASP.NET MVC applications, handling HTTP requests and managing the flow of data between models and views.

## Basic Structure
```csharp
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
```

## Key Responsibilities
- Handle HTTP requests
- Process user input
- Execute application logic
- Return appropriate responses
- Manage navigation flow

## Contents
1. [Core Concepts](02-core-concepts.md)
2. [Common Patterns](03-patterns.md)
3. [Testing](04-testing.md)
4. [Best Practices](05-best-practices.md)
5. [Security](06-security.md)
