# Controller Best Practices

## 1. Keep Controllers Thin
- Move business logic to services
- Use ViewModels for data transfer
- Focus on HTTP concerns only

## 2. Use Proper Action Names
```csharp
// Good
public IActionResult GetOrder(int id)
public IActionResult CreateProduct(ProductViewModel model)

// Avoid
public IActionResult Do(int id)
public IActionResult Handle(ProductViewModel model)
```

## 3. Implement Proper Error Handling
```csharp
public IActionResult GetOrder(int id)
{
    try
    {
        var order = _orderService.GetById(id);
        if (order == null)
            return NotFound();
            
        return View(order);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving order");
        return StatusCode(500, "Internal server error");
    }
}
```

## 4. Use Async/Await
```csharp
public async Task<IActionResult> GetOrdersAsync()
{
    var orders = await _orderService.GetAllAsync();
    return View(orders);
}
```

## 5. Implement Proper Validation
```csharp
[HttpPost]
public IActionResult Create(ProductViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }
    
    try
    {
        _productService.Create(model);
        return RedirectToAction(nameof(Index));
    }
    catch (ValidationException ex)
    {
        ModelState.AddModelError("", ex.Message);
        return View(model);
    }
}
```
