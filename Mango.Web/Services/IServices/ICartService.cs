using Mango.Web.Models;

namespace Mango.Web.Services.IServices
{
    public interface ICartService
    {
        Task<T> GetCartByUserIdAsync<T>(string userId, string token = "");
        Task<T> AddToCartAsync<T>(CartDto cartDto, string token = "");
        Task<T> UpdateCartAsync<T>(CartDto cartDto, string token = "");
        Task<T> RemoveFromCartAsync<T>(int cartId, string token = "");
        Task<T> ApplyCouponAsync<T>(CartDto cartDto, string token = "");
        Task<T> RemoveCouponAsync<T>(string userId, string token = "");
        Task<T> Checkout<T>(CartHeaderDto cartHeaderDto, string token = "");
    }
}
