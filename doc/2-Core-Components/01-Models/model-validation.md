# Model Validation in ASP.NET Core MVC

## Overview
Model validation ensures data integrity and security by:
- Validating user input
- Enforcing business rules
- Providing feedback to users
- Preventing invalid data persistence

## Built-in Validation Attributes

### Common Attributes
```csharp
public class ProductModel
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }

    [Range(0.01, 10000.00)]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; }

    [Range(0, 1000)]
    public int StockQuantity { get; set; }

    [Url]
    public string ImageUrl { get; set; }

    [EmailAddress]
    public string ContactEmail { get; set; }
}
```

### Complex Validation
```csharp
public class OrderModel
{
    [Required]
    public int Id { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Order Date")]
    public DateTime OrderDate { get; set; }

    [Required]
    [Compare("ConfirmEmail")]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [EmailAddress]
    public string ConfirmEmail { get; set; }

    [Required]
    [RegularExpression(@"^\d{5}(-\d{4})?$")]
    public string ZipCode { get; set; }

    [CreditCard]
    public string CreditCard { get; set; }

    [Phone]
    public string PhoneNumber { get; set; }
}
```

## Custom Validation

### Custom Validation Attribute
```csharp
public class FutureDateAttribute : ValidationAttribute
{
    private readonly int _daysAhead;

    public FutureDateAttribute(int daysAhead = 0)
    {
        _daysAhead = daysAhead;
    }

    protected override ValidationResult IsValid(
        object value, ValidationContext validationContext)
    {
        if (value is DateTime date)
        {
            if (date.Date < DateTime.Today.AddDays(_daysAhead))
            {
                return new ValidationResult(
                    $"Date must be at least {_daysAhead} days in the future.");
            }
        }
        return ValidationResult.Success;
    }
}

// Usage
public class EventModel
{
    [Required]
    public string Title { get; set; }

    [Required]
    [FutureDate(7)]
    [DataType(DataType.Date)]
    public DateTime EventDate { get; set; }
}
```

### IValidatableObject Implementation
```csharp
public class ReservationModel : IValidatableObject
{
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int GuestCount { get; set; }
    public bool IsBusinessTrip { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (CheckOut <= CheckIn)
        {
            yield return new ValidationResult(
                "Check-out date must be after check-in date",
                new[] { nameof(CheckOut) });
        }

        if (GuestCount < 1)
        {
            yield return new ValidationResult(
                "At least one guest is required",
                new[] { nameof(GuestCount) });
        }

        if (IsBusinessTrip && GuestCount > 2)
        {
            yield return new ValidationResult(
                "Business trips are limited to 2 guests",
                new[] { nameof(GuestCount), nameof(IsBusinessTrip) });
        }
    }
}
```

## Client-Side Validation

### Setup
```javascript
// In _Layout.cshtml
<script src="~/lib/jquery/dist/jquery.min.js"></script>
<script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
<script src="~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js"></script>
```

### Custom Client Validation
```javascript
// Custom client-side validator
$.validator.addMethod("futureDate", function (value, element, params) {
    if (!value) return true;

    var today = new Date();
    var inputDate = new Date(value);
    var daysAhead = parseInt(params);

    today.setHours(0, 0, 0, 0);
    inputDate.setHours(0, 0, 0, 0);

    var minDate = new Date();
    minDate.setDate(today.getDate() + daysAhead);

    return inputDate >= minDate;
});

$.validator.unobtrusive.adapters.add("futuredate", ["daysahead"], function (options) {
    options.rules["futureDate"] = options.params.daysahead;
    options.messages["futureDate"] = options.message;
});
```

## Remote Validation

### Controller Implementation
```csharp
public class UserController : Controller
{
    private readonly IUserService _userService;

    [AcceptVerbs("GET", "POST")]
    public async Task<IActionResult> IsEmailAvailable(string email)
    {
        var exists = await _userService.IsEmailRegisteredAsync(email);
        return Json(!exists);
    }
}

public class RegistrationModel
{
    [Required]
    [EmailAddress]
    [Remote(
        action: nameof(UserController.IsEmailAvailable),
        controller: "User",
        ErrorMessage = "Email already registered.")]
    public string Email { get; set; }
}
```

## Model State Validation

### Controller Validation
```csharp
public class ProductController : Controller
{
    private readonly IProductService _productService;

    [HttpPost]
    public async Task<IActionResult> Create(ProductModel model)
    {
        if (!ModelState.IsValid)
        {
            // Add custom model state errors
            ModelState.AddModelError(string.Empty, 
                "Please correct the errors and try again.");
            return View(model);
        }

        try
        {
            await _productService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, 
                "An error occurred while saving the product.");
            return View(model);
        }
    }
}
```

### Custom Model Validation Service
```csharp
public interface IModelValidationService
{
    Task<ValidationResult> ValidateModelAsync<T>(T model) 
        where T : class;
}

public class ModelValidationService : IModelValidationService
{
    private readonly IServiceProvider _serviceProvider;

    public ModelValidationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<ValidationResult> ValidateModelAsync<T>(T model) 
        where T : class
    {
        var context = new ValidationContext(
            model, _serviceProvider, null);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            model, context, results, validateAllProperties: true);

        if (model is IValidatableObject validatable)
        {
            results.AddRange(validatable.Validate(context));
            isValid = !results.Any();
        }

        return new ValidationResult
        {
            IsValid = isValid,
            Errors = results
        };
    }
}
```

## Validation Display

### View Implementation
```cshtml
@model ProductModel

<form asp-action="Create">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>
    
    <div class="form-group">
        <label asp-for="Name"></label>
        <input asp-for="Name" class="form-control" />
        <span asp-validation-for="Name" class="text-danger"></span>
    </div>

    <div class="form-group">
        <label asp-for="Price"></label>
        <input asp-for="Price" class="form-control" />
        <span asp-validation-for="Price" class="text-danger"></span>
    </div>

    <button type="submit" class="btn btn-primary">Create</button>
</form>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

## Best Practices

### 1. Validation Strategy
- Use appropriate validation attributes
- Implement custom validation when needed
- Consider both client and server validation
- Handle validation errors gracefully

### 2. Error Messages
- Provide clear, user-friendly messages
- Support localization
- Include validation context
- Be specific but secure

### 3. Security
- Always validate on the server
- Don't reveal sensitive information
- Protect against over-posting
- Use anti-forgery tokens

### 4. Performance
- Use client validation when appropriate
- Optimize remote validation calls
- Cache validation results when possible
- Consider validation complexity

## Related Topics
- [Data Annotations](data-annotations.md)
- [Forms and Input](../03-Views/forms-and-html.md)
- [Error Handling](../3-Advanced-Concepts/04-Error-Handling/error-handling.md)
- [Security Best Practices](../4-Best-Practices/02-Security/security-validation.md)