using MockEcommerce.Api.Models;

namespace MockEcommerce.Api.Services;

public interface IProductService
{
    IEnumerable<Product> GetAll();
    Product? GetById(int id);
}
