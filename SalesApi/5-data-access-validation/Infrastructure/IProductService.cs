using SalesApi.Domain.Model;

namespace SalesApi.Infrastructure;

public interface IProductService
{
    Task<(ReadDataResult, List<Product>?)> GetAllAvailableProducts();
    Task<(ReadDataResult, Product?)> GetWith(ProductId id);
}

public enum ReadDataResult
{
    Success = 0,
    NotFound,
    NoAccessToData,
    NoAccessToOperation,
    InvalidData
}

public class ProductService(IProductRepository productRepository, IPermissionService permissionService) : IProductService
{
    public async Task<(ReadDataResult, List<Product>?)> GetAllAvailableProducts()
    {
        if (!permissionService.CanReadProducts)
        {
            return (ReadDataResult.NoAccessToOperation, null);
        }

        var products = await productRepository.GetAllAvailable();

        var allowedProducts = products.Where(product => permissionService.HasPermissionToMarket(product.MarketId)).ToList();

        return (ReadDataResult.Success, allowedProducts);
    }

    public async Task<(ReadDataResult, Product?)> GetWith(ProductId id)
    {
        if (!permissionService.CanReadProducts)
        {
            return (ReadDataResult.NoAccessToOperation, null);
        }

        var product = await productRepository.GetBy(id);

        if (product == null)
        {
            return (ReadDataResult.NotFound, null);
        }

        if (!permissionService.HasPermissionToMarket(product.MarketId))
        {
            return (ReadDataResult.NoAccessToData, null);
        }

        return (ReadDataResult.Success, product);
    }
}