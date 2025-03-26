# Models in ASP.NET Core MVC

## Table of Contents
1. [Domain Models](domain-models.md)
2. [View Models](view-models.md)
3. [Data Annotations](data-annotations.md)
4. [Model Binding](model-binding.md)
5. [Model Validation](model-validation.md)
6. [Entity Framework Integration](ef-integration.md)

## Overview

Models in ASP.NET Core MVC serve multiple purposes:
- Represent business data and rules
- Define the shape of data for views
- Provide validation logic
- Map between different data representations

## Quick Start Examples

### Domain Model
```csharp
public class Customer
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    public DateTime RegisteredDate { get; set; }

    public virtual ICollection<Order> Orders { get; set; }
}
```

### View Model
```csharp
public class CustomerViewModel
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string FormattedRegistrationDate { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
}
```

### DTO (Data Transfer Object)
```csharp
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

## Model Types

### 1. Domain Models
- Represent business entities
- Include business logic and validation
- Map to database structure
- Contain relationships between entities

### 2. View Models
- Specific to view requirements
- Combine data from multiple sources
- Include display formatting
- Handle form submissions

### 3. DTOs
- Simple data containers
- Used for API responses
- Limit data exposure
- Optimize data transfer

## Best Practices

### 1. Separation of Concerns
- Keep domain models focused on business logic
- Use view models for presentation
- Avoid mixing concerns

### 2. Validation
- Use data annotations
- Implement custom validation when needed
- Validate at both client and server

### 3. Security
- Never expose sensitive data
- Use DTOs for API responses
- Implement proper authorization

### 4. Performance
- Use appropriate data types
- Consider lazy loading
- Implement caching where appropriate

## Common Patterns

### Repository Pattern
```csharp
public interface ICustomerRepository
{
    Task<Customer> GetByIdAsync(int id);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer> CreateAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(int id);
}
```

### Service Layer
```csharp
public interface ICustomerService
{
    Task<CustomerViewModel> GetCustomerDetailsAsync(int id);
    Task<IEnumerable<CustomerListItem>> GetActiveCustomersAsync();
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request);
}
```

## Related Topics
- [Model Binding](model-binding.md)
- [Data Validation](model-validation.md)
- [Entity Framework Core](ef-integration.md)