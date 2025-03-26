# Entity Framework Integration in ASP.NET Core MVC

## Overview
Entity Framework Core (EF Core) is the primary ORM for ASP.NET Core applications. This guide covers:
- Model configuration
- Relationships
- Migrations
- Common patterns
- Best practices

## Model Configuration

### Basic Entity Configuration
```csharp
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public CustomerStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual ICollection<Order> Orders { get; set; }
}

public class ApplicationDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Email)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(e => e.Status)
                  .HasConversion<string>();

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
```

### Fluent Configuration Class
```csharp
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(e => e.Email)
               .IsRequired()
               .HasMaxLength(255);

        builder.HasIndex(e => e.Email)
               .IsUnique();

        builder.Property(e => e.Status)
               .HasConversion<string>();

        builder.HasMany(e => e.Orders)
               .WithOne(e => e.Customer)
               .HasForeignKey(e => e.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
```

## Relationships

### One-to-Many Relationship
```csharp
public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
    
    public ICollection<OrderItem> Items { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    public Order Order { get; set; }
    public Product Product { get; set; }
}
```

### Many-to-Many Relationship
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public ICollection<ProductCategory> ProductCategories { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<ProductCategory> ProductCategories { get; set; }
}

public class ProductCategory
{
    public int ProductId { get; set; }
    public Product Product { get; set; }
    
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}

// Configuration
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasMany(p => p.ProductCategories)
               .WithOne(pc => pc.Product)
               .HasForeignKey(pc => pc.ProductId);
    }
}
```

## Value Objects

### Implementing Value Objects
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
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    protected override bool Equals(object obj)
    {
        var other = obj as Address;
        return other != null &&
               Street == other.Street &&
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

// Using value objects in entities
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address BillingAddress { get; set; }
    public Address ShippingAddress { get; set; }
}

// Configuration
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.OwnsOne(c => c.BillingAddress);
        builder.OwnsOne(c => c.ShippingAddress);
    }
}
```

## Repository Pattern

### Base Repository
```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task SaveChangesAsync();
}

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
```

### Specific Repository
```csharp
public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer> GetWithOrdersAsync(int id);
    Task<IEnumerable<Customer>> GetActiveCustomersAsync();
}

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Customer> GetWithOrdersAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
    {
        return await _dbSet
            .Where(c => c.Status == CustomerStatus.Active)
            .ToListAsync();
    }
}
```

## Unit of Work Pattern

### Implementation
```csharp
public interface IUnitOfWork : IDisposable
{
    ICustomerRepository Customers { get; }
    IOrderRepository Orders { get; }
    Task<int> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private ICustomerRepository _customers;
    private IOrderRepository _orders;
    private bool _disposed;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public ICustomerRepository Customers
    {
        get { return _customers ??= new CustomerRepository(_context); }
    }

    public IOrderRepository Orders
    {
        get { return _orders ??= new OrderRepository(_context); }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
```

## Best Practices

### 1. Model Design
- Use appropriate data types
- Implement proper relationships
- Consider performance impact
- Use value objects where appropriate

### 2. Performance
- Use async operations
- Implement lazy loading carefully
- Consider query optimization
- Use appropriate indexes

### 3. Data Access
- Implement repository pattern
- Use unit of work pattern
- Handle concurrency
- Manage transactions properly

### 4. Security
- Validate inputs
- Protect sensitive data
- Implement proper authorization
- Use secure connections

## Related Topics
- [Domain Models](domain-models.md)
- [Data Annotations](data-annotations.md)
- [Model Validation](model-validation.md)