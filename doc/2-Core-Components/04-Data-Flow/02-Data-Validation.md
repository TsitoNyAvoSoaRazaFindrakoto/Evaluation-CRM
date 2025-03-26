# Data Validation in ASP.NET MVC

Data validation ensures that data meets specific requirements before processing.

## Validation Attributes

```csharp
public class CustomerViewModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    [Range(18, 100)]
    public int Age { get; set; }
}
```

## Custom Validation

```csharp
public class CustomValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(
        object value, ValidationContext validationContext)
    {
        // Custom validation logic
    }
}
```

## Model State Validation

```csharp
[HttpPost]
public IActionResult Create(CustomerViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }
    // Process valid model
}
```

## Client-Side Validation
```cshtml
@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```
