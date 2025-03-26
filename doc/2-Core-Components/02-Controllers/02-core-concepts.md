# Controller Core Concepts

## Action Methods
Action methods are public methods that handle incoming requests:
```csharp
public IActionResult Details(int id)
{
    var product = _productService.GetById(id);
    return View(product);
}
```

## Return Types
Common return types include:
- ViewResult
- JsonResult
- ContentResult
- FileResult
- RedirectResult

## Parameter Binding
```csharp
// Route binding
[Route("products/{id}")]
public IActionResult GetProduct(int id)

// Query string
public IActionResult Search([FromQuery]string term)

// Form data
[HttpPost]
public IActionResult Create([FromForm]ProductViewModel model)
```

## Dependency Injection
```csharp
public class OrderController : Controller
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }
}
```
