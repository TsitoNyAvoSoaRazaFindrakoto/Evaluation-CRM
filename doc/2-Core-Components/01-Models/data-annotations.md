# Data Annotations in ASP.NET Core MVC

## Overview
Data annotations provide a way to:
- Configure validation rules
- Specify display metadata
- Configure model binding
- Influence database schema generation

## Validation Annotations

### Basic Validation
```csharp
public class RegisterViewModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; }
}
```

### Numeric Validation
```csharp
public class ProductInputModel
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    [Display(Name = "Quantity in Stock")]
    public int StockQuantity { get; set; }

    [Range(0, 100)]
    [Display(Name = "Discount Percentage")]
    public decimal DiscountPercentage { get; set; }
}
```

### String Validation
```csharp
public class ArticleModel
{
    [Required]
    [StringLength(200, MinimumLength = 10)]
    public string Title { get; set; }

    [Required]
    [MinLength(100)]
    [MaxLength(5000)]
    [DataType(DataType.MultilineText)]
    public string Content { get; set; }

    [RegularExpression(@"^[a-zA-Z0-9\-]+$")]
    [StringLength(50)]
    [Display(Name = "URL Slug")]
    public string Slug { get; set; }
}
```

## Display Annotations

### Basic Display
```csharp
public class CustomerViewModel
{
    [Display(Name = "Customer ID")]
    public int Id { get; set; }

    [Display(Name = "Full Name")]
    [DisplayFormat(NullDisplayText = "No name provided")]
    public string Name { get; set; }

    [Display(Name = "Date of Birth")]
    [DisplayFormat(DataFormatString = "{0:d}")]
    public DateTime DateOfBirth { get; set; }

    [Display(Name = "Active Customer")]
    [DisplayFormat(NullDisplayText = "Status unknown")]
    public bool? IsActive { get; set; }
}
```

### Advanced Display Formatting
```csharp
public class InvoiceViewModel
{
    [Display(Name = "Invoice Number")]
    [DisplayFormat(DataFormatString = "INV-{0:D6}")]
    public int InvoiceNumber { get; set; }

    [Display(Name = "Issue Date")]
    [DisplayFormat(DataFormatString = "{0:dd MMMM yyyy}")]
    public DateTime IssueDate { get; set; }

    [Display(Name = "Due Amount")]
    [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
    public decimal Amount { get; set; }

    [Display(Name = "Payment Status")]
    [DisplayFormat(NullDisplayText = "Pending")]
    public string PaymentStatus { get; set; }
}
```

## Custom Annotations

### Custom Validation Attribute
```csharp
public class MinAgeAttribute : ValidationAttribute
{
    private readonly int _minimumAge;

    public MinAgeAttribute(int minimumAge)
    {
        _minimumAge = minimumAge;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is DateTime dateOfBirth)
        {
            var age = DateTime.Today.Year - dateOfBirth.Year;
            
            // Adjust age if birthday hasn't occurred this year
            if (dateOfBirth.Date > DateTime.Today.AddYears(-age))
                age--;

            if (age >= _minimumAge)
                return ValidationResult.Success;

            return new ValidationResult($"Age must be at least {_minimumAge} years old");
        }

        return new ValidationResult("Invalid date of birth");
    }
}

// Usage
public class UserProfile
{
    [Required]
    [MinAge(18, ErrorMessage = "Must be at least 18 years old")]
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }
}
```

### Custom Display Attribute
```csharp
public class MaskAttribute : Attribute
{
    public string Pattern { get; }

    public MaskAttribute(string pattern)
    {
        Pattern = pattern;
    }
}

public class CreditCardViewModel
{
    [Required]
    [CreditCard]
    [Mask("****-****-****-####")]
    [Display(Name = "Credit Card Number")]
    public string CardNumber { get; set; }
}
```

## Database Annotations

### Table Configuration
```csharp
[Table("Customers")]
public class Customer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("CustomerName", TypeName = "nvarchar(100)")]
    public string Name { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CreditLimit { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }
}
```

### Relationship Configuration
```csharp
public class Order
{
    [Key]
    public int OrderId { get; set; }

    [Required]
    [ForeignKey("Customer")]
    public int CustomerId { get; set; }

    [Required]
    public DateTime OrderDate { get; set; }

    [ForeignKey("CustomerId")]
    public virtual Customer Customer { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<OrderItem> OrderItems { get; set; }
}
```

## Best Practices

### 1. Validation Strategy
- Use built-in attributes when possible
- Combine multiple validation attributes
- Implement custom validation for complex rules
- Consider client-side validation impact

### 2. Display Guidelines
- Provide meaningful display names
- Use appropriate format strings
- Consider localization requirements
- Handle null values appropriately

### 3. Database Considerations
- Use appropriate column types
- Consider index requirements
- Plan relationships carefully
- Handle concurrency properly

### 4. Security
- Validate input thoroughly
- Protect sensitive data
- Consider GDPR requirements
- Implement proper authorization

## Common Patterns

### Form Validation
```csharp
public class ContactForm
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Your Name")]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; }

    [Required]
    [StringLength(500)]
    [DataType(DataType.MultilineText)]
    public string Message { get; set; }

    [Display(Name = "Preferred Contact Time")]
    [DataType(DataType.Time)]
    public DateTime? PreferredTime { get; set; }

    [Phone]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; }
}
```

### API Model Validation
```csharp
public class ApiRequest
{
    [Required]
    [JsonPropertyName("api_key")]
    public string ApiKey { get; set; }

    [Required]
    [JsonPropertyName("request_id")]
    [RegularExpression(@"^[A-Za-z0-9\-]{36}$")]
    public string RequestId { get; set; }

    [Range(1, 100)]
    [JsonPropertyName("page_size")]
    public int PageSize { get; set; } = 10;

    [Required]
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}
```

## Related Topics
- [Model Validation](model-validation.md)
- [Entity Framework Integration](ef-integration.md)
- [Model Binding](model-binding.md)