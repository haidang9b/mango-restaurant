using Mango.Services.ShoppingCartAPI.Models.Dto;

namespace Mango.Services.ShoppingCartAPI.Repositories
{
    public interface ICouponRepository
    {
        Task<CouponDto> GetCoupon(string couponCode);
    }
}
