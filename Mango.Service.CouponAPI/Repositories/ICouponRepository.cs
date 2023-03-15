using Mango.Service.CouponAPI.Models.Dtos;

namespace Mango.Service.CouponAPI.Repositories
{
    public interface ICouponRepository
    {
        Task<CouponDto> GetCouponByCode(string code);
    }
}
