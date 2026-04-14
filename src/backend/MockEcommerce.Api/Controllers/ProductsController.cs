using Microsoft.AspNetCore.Mvc;
using MockEcommerce.Api.Models;
using MockEcommerce.Api.Services;

namespace MockEcommerce.Api.Controllers;

/// <summary>
/// Provides endpoints for browsing the product catalog.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    /// <summary>Returns all products in the catalog.</summary>
    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetAll()
    {
        return Ok(productService.GetAll());
    }

    /// <summary>Returns a single product by its unique identifier.</summary>
    /// <param name="id">The product ID.</param>
    [HttpGet("{id:int}")]
    public ActionResult<Product> GetById(int id)
    {
        var product = productService.GetById(id);
        if (product is null)
            return NotFound();

        return Ok(product);
    }
}
