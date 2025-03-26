# View Models in ASP.NET Core MVC

## Overview
View models are specialized model classes designed to:
- Transfer data between controllers and views
- Handle form submissions
- Separate domain logic from presentation
- Provide type safety in views
- Implement display and validation logic

## Basic View Models

### Simple View Model
```csharp
public class CustomerViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "Full Name")]
    public string Name { get; set; }
    
    [Display(Name = "Email Address")]
    [EmailAddress]
    public string Email { get; set; }
    
    [Display(Name = "Phone Number")]
    [Phone]
    public string PhoneNumber { get; set; }
    
    [Display(Name = "Account Status")]
    public string Status { get; set; }
    
    [Display(Name = "Total Orders")]
    public int OrderCount { get; set; }
}

// Controller Usage
public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IMapper _mapper;

    public async Task<IActionResult> Details(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        var viewModel = _mapper.Map<CustomerViewModel>(customer);
        return View(viewModel);
    }
}
```

### Form View Model
```csharp
public class CreateCustomerViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; }

    [Required]
    [Phone]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; }

    [Required]
    [StringLength(200)]
    public string Address { get; set; }

    [Display(Name = "Subscribe to Newsletter")]
    public bool NewsletterSubscription { get; set; }
}

// Controller
public class CustomerController : Controller
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var customer = _mapper.Map<Customer>(model);
        await _customerService.CreateAsync(customer);
        
        return RedirectToAction(nameof(Index));
    }
}
```

## Advanced View Models

### Nested View Models
```csharp
public class OrderViewModel
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public CustomerSummaryViewModel Customer { get; set; }
    public List<OrderItemViewModel> Items { get; set; }
    public OrderTotalsViewModel Totals { get; set; }
}

public class CustomerSummaryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class OrderItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}

public class OrderTotalsViewModel
{
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total => Subtotal + TaxAmount + ShippingCost;
}
```

### List View Models
```csharp
public class CustomerListViewModel
{
    public List<CustomerSummaryViewModel> Customers { get; set; }
    public PaginationViewModel Pagination { get; set; }
    public CustomerFilterViewModel Filter { get; set; }
}

public class PaginationViewModel
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}

public class CustomerFilterViewModel
{
    public string SearchTerm { get; set; }
    public CustomerStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
```

## AutoMapper Integration

### Profile Configuration
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Customer, CustomerViewModel>()
            .ForMember(dest => dest.OrderCount, 
                      opt => opt.MapFrom(src => src.Orders.Count));

        CreateMap<CreateCustomerViewModel, Customer>();

        CreateMap<Order, OrderViewModel>()
            .ForMember(dest => dest.Totals, 
                      opt => opt.MapFrom(src => new OrderTotalsViewModel
                      {
                          Subtotal = src.Items.Sum(i => i.Quantity * i.UnitPrice),
                          TaxAmount = src.TaxAmount,
                          ShippingCost = src.ShippingCost
                      }));

        CreateMap<OrderItem, OrderItemViewModel>();
    }
}
```

### Service Implementation
```csharp
public class CustomerViewModelService
{
    private readonly ICustomerService _customerService;
    private readonly IMapper _mapper;

    public CustomerViewModelService(
        ICustomerService customerService,
        IMapper mapper)
    {
        _customerService = customerService;
        _mapper = mapper;
    }

    public async Task<CustomerListViewModel> GetCustomerListAsync(
        CustomerFilterViewModel filter, 
        int page = 1, 
        int pageSize = 10)
    {
        var query = _customerService.GetQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(c => 
                c.Name.Contains(filter.SearchTerm) || 
                c.Email.Contains(filter.SearchTerm));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(c => c.Status == filter.Status);
        }

        // Get total count
        var totalItems = await query.CountAsync();

        // Apply pagination
        var customers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<CustomerSummaryViewModel>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new CustomerListViewModel
        {
            Customers = customers,
            Filter = filter,
            Pagination = new PaginationViewModel
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            }
        };
    }
}
```

## Best Practices

### 1. View Model Design
- Keep models focused and specific
- Include only needed properties
- Use appropriate data types
- Implement proper validation

### 2. Mapping Strategy
- Use AutoMapper for complex mappings
- Create specific mapping profiles
- Handle nested objects properly
- Consider performance impact

### 3. Validation
- Implement proper validation attributes
- Use custom validation when needed
- Handle validation errors gracefully
- Consider client-side validation

### 4. Security
- Never expose sensitive data
- Validate all inputs
- Implement proper authorization
- Use anti-forgery tokens

## Common Patterns

### Search and Filter
```csharp
public class ProductSearchViewModel
{
    public string SearchTerm { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<int> CategoryIds { get; set; }
    public SortOption SortBy { get; set; }
    public List<ProductViewModel> Results { get; set; }
    public PaginationViewModel Pagination { get; set; }
}

public enum SortOption
{
    [Display(Name = "Name (A-Z)")]
    NameAsc,
    [Display(Name = "Name (Z-A)")]
    NameDesc,
    [Display(Name = "Price (Low-High)")]
    PriceLow,
    [Display(Name = "Price (High-Low)")]
    PriceHigh,
    [Display(Name = "Newest First")]
    Newest
}
```

### Form Handling
```csharp
public class CheckoutViewModel
{
    public ShippingAddressViewModel ShippingAddress { get; set; }
    public BillingAddressViewModel BillingAddress { get; set; }
    public PaymentViewModel Payment { get; set; }
    public bool SameAsShipping { get; set; }
    public OrderSummaryViewModel OrderSummary { get; set; }
}

public class PaymentViewModel
{
    [Required]
    [CreditCard]
    [Display(Name = "Card Number")]
    public string CardNumber { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Cardholder Name")]
    public string CardholderName { get; set; }

    [Required]
    [RegularExpression(@"^\d{2}/\d{2}$")]
    [Display(Name = "Expiry (MM/YY)")]
    public string ExpiryDate { get; set; }

    [Required]
    [RegularExpression(@"^\d{3,4}$")]
    [Display(Name = "CVV")]
    public string Cvv { get; set; }
}
```

## Related Topics
- [Model Binding](model-binding.md)
- [Model Validation](model-validation.md)
- [Forms and HTML Helpers](../03-Views/forms-and-html.md)
- [Data Annotations](data-annotations.md)