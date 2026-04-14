using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MockEcommerce.Api.Endpoints;
using MockEcommerce.Api.Models;

namespace MockEcommerce.Api.Tests.Endpoints;

public class CartEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CartEndpointTests(WebApplicationFactory<Program> factory)
    {
        // Create a new client per test class instance (xUnit creates a new instance per test)
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCart_WhenEmpty_ReturnsOkWithEmptyList()
    {
        var response = await _client.GetAsync("/api/cart");

        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<List<CartItem>>();
        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public async Task AddToCart_WithValidProduct_ReturnsCreatedWithCartItem()
    {
        var request = new AddToCartRequest(1, 2);

        var response = await _client.PostAsJsonAsync("/api/cart", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var item = await response.Content.ReadFromJsonAsync<CartItem>();
        Assert.NotNull(item);
        Assert.Equal(1, item.ProductId);
        Assert.Equal(2, item.Quantity);
        Assert.Equal("Wireless Headphones", item.ProductName);
        Assert.Equal(79.99m, item.UnitPrice);
    }

    [Fact]
    public async Task AddToCart_WithInvalidProduct_ReturnsNotFound()
    {
        var request = new AddToCartRequest(9999, 1);

        var response = await _client.PostAsJsonAsync("/api/cart", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddToCart_WithZeroQuantity_ReturnsBadRequest()
    {
        var request = new AddToCartRequest(1, 0);

        var response = await _client.PostAsJsonAsync("/api/cart", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddToCart_WithNegativeQuantity_ReturnsBadRequest()
    {
        var request = new AddToCartRequest(1, -1);

        var response = await _client.PostAsJsonAsync("/api/cart", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddToCart_ExistingProduct_IncrementsQuantity()
    {
        await _client.PostAsJsonAsync("/api/cart", new AddToCartRequest(2, 2));

        var response = await _client.PostAsJsonAsync("/api/cart", new AddToCartRequest(2, 3));

        response.EnsureSuccessStatusCode();
        var item = await response.Content.ReadFromJsonAsync<CartItem>();
        Assert.NotNull(item);
        Assert.Equal(2, item.ProductId);
        Assert.Equal(5, item.Quantity);
    }

    [Fact]
    public async Task RemoveFromCart_WithExistingItem_ReturnsNoContent()
    {
        await _client.PostAsJsonAsync("/api/cart", new AddToCartRequest(3, 1));

        var response = await _client.DeleteAsync("/api/cart/3");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RemoveFromCart_WithNonExistingItem_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/cart/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ClearCart_ReturnsNoContent()
    {
        await _client.PostAsJsonAsync("/api/cart", new AddToCartRequest(4, 1));
        await _client.PostAsJsonAsync("/api/cart", new AddToCartRequest(5, 1));

        var response = await _client.DeleteAsync("/api/cart");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync("/api/cart");
        getResponse.EnsureSuccessStatusCode();
        var items = await getResponse.Content.ReadFromJsonAsync<List<CartItem>>();
        Assert.NotNull(items);
        Assert.Empty(items);
    }
}
