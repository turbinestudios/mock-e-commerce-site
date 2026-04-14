using MockEcommerce.Api.Models;
using MockEcommerce.Api.Services;

namespace MockEcommerce.Api.Tests.Services;

public class InMemoryCartServiceTests
{
    private readonly InMemoryCartService _service = new();

    [Fact]
    public void GetAll_WhenEmpty_ReturnsEmptyCollection()
    {
        var items = _service.GetAll();

        Assert.Empty(items);
    }

    [Fact]
    public void Add_NewItem_ReturnsAddedItem()
    {
        var item = CreateCartItem(1, "Widget", 9.99m, 2);

        var result = _service.Add(item);

        Assert.Equal(1, result.ProductId);
        Assert.Equal(2, result.Quantity);
    }

    [Fact]
    public void Add_ExistingProduct_IncrementsQuantity()
    {
        _service.Add(CreateCartItem(1, "Widget", 9.99m, 2));

        var result = _service.Add(CreateCartItem(1, "Widget", 9.99m, 3));

        Assert.Equal(5, result.Quantity);
    }

    [Fact]
    public void GetByProductId_WithExistingProduct_ReturnsItem()
    {
        _service.Add(CreateCartItem(1, "Widget", 9.99m, 1));

        var item = _service.GetByProductId(1);

        Assert.NotNull(item);
        Assert.Equal(1, item.ProductId);
    }

    [Fact]
    public void GetByProductId_WithNonExistingProduct_ReturnsNull()
    {
        var item = _service.GetByProductId(999);

        Assert.Null(item);
    }

    [Fact]
    public void Remove_ExistingProduct_ReturnsTrueAndRemovesItem()
    {
        _service.Add(CreateCartItem(1, "Widget", 9.99m, 1));

        var removed = _service.Remove(1);

        Assert.True(removed);
        Assert.Null(_service.GetByProductId(1));
    }

    [Fact]
    public void Remove_NonExistingProduct_ReturnsFalse()
    {
        var removed = _service.Remove(999);

        Assert.False(removed);
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        _service.Add(CreateCartItem(1, "Widget", 9.99m, 1));
        _service.Add(CreateCartItem(2, "Gadget", 19.99m, 3));

        _service.Clear();

        Assert.Empty(_service.GetAll());
    }

    [Fact]
    public void GetAll_ReturnsSnapshotNotLiveReference()
    {
        _service.Add(CreateCartItem(1, "Widget", 9.99m, 1));

        var snapshot = _service.GetAll().ToList();
        _service.Clear();

        Assert.Single(snapshot);
        Assert.Empty(_service.GetAll());
    }

    private static CartItem CreateCartItem(int productId, string name, decimal price, int quantity) =>
        new()
        {
            ProductId = productId,
            ProductName = name,
            UnitPrice = price,
            Quantity = quantity
        };
}
