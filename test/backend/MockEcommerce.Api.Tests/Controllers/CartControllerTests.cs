using Microsoft.AspNetCore.Mvc;
using MockEcommerce.Api.Controllers;
using MockEcommerce.Api.Models;
using MockEcommerce.Api.Services;

namespace MockEcommerce.Api.Tests.Controllers;

public class CartControllerTests
{
    private readonly CartController _controller;
    private readonly ICartService _cartService;
    private readonly IProductService _productService;

    public CartControllerTests()
    {
        _productService = new MockProductService();
        _cartService = new InMemoryCartService();
        _controller = new CartController(_productService, _cartService);
    }

    [Fact]
    public void GetCart_WhenEmpty_ReturnsOkWithEmptyList()
    {
        var result = _controller.GetCart();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IEnumerable<CartItem>>(ok.Value);
        Assert.Empty(items);
    }

    [Fact]
    public void AddToCart_WithValidProduct_ReturnsCreatedWithCartItem()
    {
        var request = new AddToCartRequest(1, 2);

        var result = _controller.AddToCart(request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var item = Assert.IsType<CartItem>(created.Value);
        Assert.Equal(1, item.ProductId);
        Assert.Equal(2, item.Quantity);
        Assert.Equal("Wireless Headphones", item.ProductName);
        Assert.Equal(79.99m, item.UnitPrice);
    }

    [Fact]
    public void AddToCart_WithInvalidProduct_ReturnsNotFound()
    {
        var request = new AddToCartRequest(9999, 1);

        var result = _controller.AddToCart(request);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void AddToCart_WithZeroQuantity_ReturnsBadRequest()
    {
        var request = new AddToCartRequest(1, 0);

        var result = _controller.AddToCart(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void AddToCart_WithNegativeQuantity_ReturnsBadRequest()
    {
        var request = new AddToCartRequest(1, -1);

        var result = _controller.AddToCart(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void AddToCart_ExistingProduct_IncrementsQuantity()
    {
        _controller.AddToCart(new AddToCartRequest(1, 2));

        var result = _controller.AddToCart(new AddToCartRequest(1, 3));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var item = Assert.IsType<CartItem>(ok.Value);
        Assert.Equal(1, item.ProductId);
        Assert.Equal(5, item.Quantity);
    }

    [Fact]
    public void RemoveFromCart_WithExistingItem_ReturnsNoContent()
    {
        _controller.AddToCart(new AddToCartRequest(1, 1));

        var result = _controller.RemoveFromCart(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void RemoveFromCart_WithNonExistingItem_ReturnsNotFound()
    {
        var result = _controller.RemoveFromCart(9999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void ClearCart_ReturnsNoContent()
    {
        _controller.AddToCart(new AddToCartRequest(1, 1));
        _controller.AddToCart(new AddToCartRequest(2, 1));

        var result = _controller.ClearCart();

        Assert.IsType<NoContentResult>(result);
        var getResult = _controller.GetCart();
        var ok = Assert.IsType<OkObjectResult>(getResult.Result);
        var items = Assert.IsAssignableFrom<IEnumerable<CartItem>>(ok.Value);
        Assert.Empty(items);
    }
}
