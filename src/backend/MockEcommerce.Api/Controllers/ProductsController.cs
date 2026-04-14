using Microsoft.AspNetCore.Mvc;
using MockEcommerce.Api.Models;
using MockEcommerce.Api.Services;

namespace MockEcommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetAll()
    {
        return Ok(productService.GetAll());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Product> GetById(int id)
    {
        var product = productService.GetById(id);
        if (product is null)
            return NotFound();

        return Ok(product);
    }
}
