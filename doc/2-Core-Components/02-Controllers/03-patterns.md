# Controller Patterns

## PRG Pattern (Post-Redirect-Get)
```csharp
[HttpPost]
public IActionResult Create(ProductViewModel model)
{
    if (ModelState.IsValid)
    {
        _productService.Create(model);
        return RedirectToAction("Index");
    }
    return View(model);
}
```

## API Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductApiController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Product>> Get()
    {
        return Ok(_productService.GetAll());
    }
}
```

## CRUD Operations
```csharp
public class ProductController : Controller
{
    private readonly IProductService _service;

    // Create
    [HttpPost]
    public IActionResult Create(ProductViewModel model)

    // Read
    public IActionResult Details(int id)

    // Update
    [HttpPut]
    public IActionResult Edit(int id, ProductViewModel model)

    // Delete
    [HttpDelete]
    public IActionResult Delete(int id)
}
```
