# Domain Models in ASP.NET Core MVC

## Overview
Domain models represent the business entities and logic in your application. They are the core data structures that map to your database and contain business rules and validation logic.

## Structure and Best Practices

### Basic Model Structure
```csharp
public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    
    // Navigation properties
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
    public ICollection<OrderItem> Items { get; set; }
    
    // Business logic methods
    public void CalculateTotal()
    {
        TotalAmount = Items?.Sum(i => i.Quantity * i.UnitPrice) ?? 0;
    }
    
    public bool CanBeCancelled()
    {
        return Status == OrderStatus.Pending || 
               (Status == OrderStatus.Processing && 
                OrderDate.AddHours(24) > DateTime.UtcNow);
    }
}
```

### Value Objects
```csharp
public class Address
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }

    private Address() { } // For EF Core

    public Address(string street, string city, string state, 
                  string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required");
            
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public override bool Equals(object obj)
    {
        if (obj is not Address other) return false;
        
        return Street == other.Street &&
               City == other.City &&
               State == other.State &&
               PostalCode == other.PostalCode &&
               Country == other.Country;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Street, City, State, PostalCode, Country);
    }
}
```

### Entity Base Class
```csharp
public abstract class EntityBase
{
    public int Id { get; protected set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    
    public override bool Equals(object obj)
    {
        if (obj is not EntityBase other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (Id == 0 || other.Id == 0)
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }
}
```

## Domain Model Patterns

### 1. Rich Domain Models
```csharp
public class Customer : EntityBase
{
    private readonly List<Order> _orders = new();
    
    public string Name { get; private set; }
    public CustomerStatus Status { get; private set; }
    public CustomerTier Tier { get; private set; }
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    public Customer(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");
            
        Name = name;
        Status = CustomerStatus.Active;
        Tier = CustomerTier.Standard;
    }

    public void UpdateTier()
    {
        var totalSpent = _orders.Where(o => o.OrderDate >= DateTime.UtcNow.AddYears(-1))
                               .Sum(o => o.TotalAmount);
                               
        Tier = totalSpent switch
        {
            >= 10000 => CustomerTier.Platinum,
            >= 5000 => CustomerTier.Gold,
            >= 1000 => CustomerTier.Silver,
            _ => CustomerTier.Standard
        };
    }

    public void AddOrder(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
            
        if (Status != CustomerStatus.Active)
            throw new InvalidOperationException("Cannot add orders for inactive customers");
            
        _orders.Add(order);
        UpdateTier();
    }
}
```

### 2. Domain Events
```csharp
public abstract class DomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public class OrderPlacedEvent : DomainEvent
{
    public Order Order { get; }
    
    public OrderPlacedEvent(Order order)
    {
        Order = order;
    }
}

public abstract class AggregateRoot : EntityBase
{
    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

## Validation

### Data Annotations
```csharp
public class Product : EntityBase
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [StringLength(20)]
    public string SKU { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}
```

### Custom Validation
```csharp
public class Order : EntityBase, IValidatableObject
{
    public DateTime DeliveryDate { get; set; }
    public ICollection<OrderItem> Items { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DeliveryDate <= DateTime.Today)
        {
            yield return new ValidationResult(
                "Delivery date must be in the future",
                new[] { nameof(DeliveryDate) });
        }

        if (Items == null || !Items.Any())
        {
            yield return new ValidationResult(
                "Order must contain at least one item",
                new[] { nameof(Items) });
        }
    }
}
```

## Entity Framework Configuration

### Fluent API Configuration
```csharp
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderDate)
               .IsRequired();

        builder.Property(o => o.TotalAmount)
               .HasPrecision(18, 2);

        builder.HasOne(o => o.Customer)
               .WithMany(c => c.Orders)
               .HasForeignKey(o => o.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
               .WithOne()
               .HasForeignKey(oi => oi.OrderId);

        builder.Property(o => o.Status)
               .HasConversion<string>();
    }
}
```

## Best Practices

### 1. Encapsulation
- Use private setters where appropriate
- Implement business logic within the model
- Protect invariants
- Use factory methods for complex creation

### 2. Validation
- Combine DataAnnotations with IValidatableObject
- Validate at the domain level
- Use domain exceptions for business rule violations

### 3. Navigation Properties
- Define relationships clearly
- Use virtual for lazy loading
- Consider bidirectional relationships carefully

### 4. Performance
- Be mindful of lazy/eager loading
- Use appropriate data types
- Consider indexing strategies

## Related Topics
- [View Models](view-models.md)
- [Model Validation](model-validation.md)
- [Entity Framework Integration](ef-integration.md)