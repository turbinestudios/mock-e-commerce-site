using Microsoft.AspNetCore.Mvc;
using MockEcommerce.Api.Models;
using MockEcommerce.Api.Services;

namespace MockEcommerce.Api.Controllers;

/// <summary>
/// Manages the shopping cart — add, remove, list, and clear items.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CartController(IProductService productService, ICartService cartService) : ControllerBase
{
    /// <summary>Returns all items currently in the cart.</summary>
    [HttpGet]
    public ActionResult<IEnumerable<CartItem>> GetCart()
    {
        return Ok(cartService.GetAll());
    }

    /// <summary>Adds a product to the cart or increments quantity if already present.</summary>
    [HttpPost]
    public ActionResult<CartItem> AddToCart([FromBody] AddToCartRequest request)
    {
        var product = productService.GetById(request.ProductId);
        if (product is null)
            return NotFound($"Product {request.ProductId} not found.");

        if (request.Quantity <= 0)
            return BadRequest("Quantity must be greater than zero.");

        var existing = cartService.GetByProductId(request.ProductId);
        if (existing is not null)
        {
            var updated = cartService.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = request.Quantity
            });
            return Ok(updated);
        }

        var item = new CartItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = request.Quantity
        };

        cartService.Add(item);
        return CreatedAtAction(nameof(GetCart), item);
    }

    /// <summary>Removes a single product from the cart by its product ID.</summary>
    [HttpDelete("{productId:int}")]
    public IActionResult RemoveFromCart(int productId)
    {
        if (!cartService.Remove(productId))
            return NotFound();

        return NoContent();
    }

    /// <summary>Removes all items from the cart.</summary>
    [HttpDelete]
    public IActionResult ClearCart()
    {
        cartService.Clear();
        return NoContent();
    }
}

/// <summary>Request body for adding a product to the cart.</summary>
public record AddToCartRequest(int ProductId, int Quantity);
