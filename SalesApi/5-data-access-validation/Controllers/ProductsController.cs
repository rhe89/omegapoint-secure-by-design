using AutoMapper;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using SalesApi.Domain.Model;
using SalesApi.Infrastructure;

namespace SalesApi.Controllers;

[ApiController]
[Route("api/product")]
public class ProductsController(IMapper mapper, IProductRepository productRepository, IProductService productService) : ControllerBase
{
    [HttpGet("", Name = "GetProducts")]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAll()
    {
        (ReadDataResult result, IList<Product>? products) = await productService.GetAllAvailableProducts();

        switch(result)
        {
            case ReadDataResult.Success:
                return Ok(mapper.Map<IEnumerable<ProductDTO>>(products));
            case ReadDataResult.NotFound:
            case ReadDataResult.NoAccessToData:
                return NotFound();
            case ReadDataResult.NoAccessToOperation:
                return Forbid();
            case ReadDataResult.InvalidData:
                return BadRequest("Invalid data.");
            default:
                throw new InvalidOperationException($"Result kind {result} is not supported");
        }
    }

    [HttpGet("{id}", Name = "GetProduct")]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetById([FromRoute] string id)
    {
        if (!ProductId.IsValid(id))
        {
            return BadRequest("Parameter id is not well formed");
        }

        (ReadDataResult result, Product? product) = await productService.GetWith(new ProductId(id));

        switch(result)
        {
            case ReadDataResult.Success:
                return Ok(mapper.Map<ProductDTO>(product));
            case ReadDataResult.NotFound:
            case ReadDataResult.NoAccessToData:
                return NotFound();
            case ReadDataResult.NoAccessToOperation:
                return Forbid();
            case ReadDataResult.InvalidData:
                return BadRequest("Invalid data.");
            default:
                throw new InvalidOperationException($"Result kind {result} is not supported");
        }

    }
}