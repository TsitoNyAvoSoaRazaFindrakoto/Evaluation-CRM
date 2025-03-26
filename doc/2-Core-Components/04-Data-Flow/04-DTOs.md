# Data Transfer Objects (DTOs) in ASP.NET MVC

DTOs are objects used to transfer data between processes or layers of an application.

## Basic DTO Pattern

```csharp
// Domain Model
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTime CreatedDate { get; set; }
}

// DTO
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

## AutoMapper Configuration

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Customer, CustomerDto>();
        CreateMap<CustomerDto, Customer>();
    }
}
```

## Using DTOs in Controllers

```csharp
public class CustomersController : Controller
{
    private readonly IMapper _mapper;

    public CustomersController(IMapper mapper)
    {
        _mapper = mapper;
    }

    public IActionResult GetCustomer(int id)
    {
        var customer = _customerService.GetById(id);
        var customerDto = _mapper.Map<CustomerDto>(customer);
        return Ok(customerDto);
    }
}
```
