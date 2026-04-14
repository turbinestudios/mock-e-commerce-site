using Microsoft.AspNetCore.Mvc;
using MockEcommerce.Api.Controllers;
using MockEcommerce.Api.Models;
using MockEcommerce.Api.Services;

namespace MockEcommerce.Api.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        var service = new MockProductService();
        _controller = new ProductsController(service);
    }

    [Fact]
    public void GetAll_ReturnsOkWithProducts()
    {
        var result = _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var products = Assert.IsAssignableFrom<IEnumerable<Product>>(ok.Value);
        Assert.NotEmpty(products);
    }

    [Fact]
    public void GetById_WithValidId_ReturnsOkWithProduct()
    {
        var result = _controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var product = Assert.IsType<Product>(ok.Value);
        Assert.Equal(1, product.Id);
    }

    [Fact]
    public void GetById_WithInvalidId_ReturnsNotFound()
    {
        var result = _controller.GetById(9999);

        Assert.IsType<NotFoundResult>(result.Result);
    }
}
