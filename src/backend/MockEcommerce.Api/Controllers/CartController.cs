using Microsoft.AspNetCore.Mvc;
using MockEcommerce.Api.Models;
using MockEcommerce.Api.Services;

namespace MockEcommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController(IProductService productService) : ControllerBase
{
    private static readonly List<CartItem> Cart = [];

    [HttpGet]
    public ActionResult<IEnumerable<CartItem>> GetCart()
    {
        return Ok(Cart);
    }

    [HttpPost]
    public ActionResult<CartItem> AddToCart([FromBody] AddToCartRequest request)
    {
        var product = productService.GetById(request.ProductId);
        if (product is null)
            return NotFound($"Product {request.ProductId} not found.");

        if (request.Quantity <= 0)
            return BadRequest("Quantity must be greater than zero.");

        var existing = Cart.FirstOrDefault(c => c.ProductId == request.ProductId);
        if (existing is not null)
        {
            existing.Quantity += request.Quantity;
            return Ok(existing);
        }

        var item = new CartItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = request.Quantity
        };

        Cart.Add(item);
        return CreatedAtAction(nameof(GetCart), item);
    }

    [HttpDelete("{productId:int}")]
    public IActionResult RemoveFromCart(int productId)
    {
        var item = Cart.FirstOrDefault(c => c.ProductId == productId);
        if (item is null)
            return NotFound();

        Cart.Remove(item);
        return NoContent();
    }

    [HttpDelete]
    public IActionResult ClearCart()
    {
        Cart.Clear();
        return NoContent();
    }
}

public record AddToCartRequest(int ProductId, int Quantity);
