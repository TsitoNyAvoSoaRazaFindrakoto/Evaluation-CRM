# Controller Testing

## Unit Testing
```csharp
public class HomeControllerTests
{
    [Fact]
    public void Index_ReturnsViewResult()
    {
        // Arrange
        var controller = new HomeController();

        // Act
        var result = controller.Index();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_WithValidModel_RedirectsToIndex()
    {
        // Arrange
        var mockService = new Mock<IProductService>();
        var controller = new ProductController(mockService.Object);
        var model = new ProductViewModel { /* properties */ };

        // Act
        var result = await controller.Create(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }
}
```

## Integration Testing
```csharp
public class ProductControllerIntegrationTests
{
    private readonly TestServer _server;
    private readonly HttpClient _client;

    public ProductControllerIntegrationTests()
    {
        _server = new TestServer(new WebHostBuilder()
            .UseStartup<Startup>());
        _client = _server.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
    }
}
```
