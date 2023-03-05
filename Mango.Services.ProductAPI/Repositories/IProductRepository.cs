using Mango.Services.ProductAPI.Models.Dtos;

namespace Mango.Services.ProductAPI.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductDto>> GetProducts();
        Task<ProductDto> GetProductById(int id);
        Task<ProductDto> SaveProduct(ProductDto product);
        Task<bool> DeleteProduct(int id);
    }
}
