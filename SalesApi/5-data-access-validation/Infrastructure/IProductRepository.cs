using SalesApi.Domain.Model;

namespace SalesApi.Infrastructure;

public interface IProductRepository
{
    Task<List<Product>> GetAllAvailable();
    Task<Product?> GetBy(ProductId productId);
    Task SaveProduct(Product product);
}