using AutoMapper;
using Mango.Service.CouponAPI.DbContexts;
using Mango.Service.CouponAPI.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Mango.Service.CouponAPI.Repositories
{
    public class CouponRepository : ICouponRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private IMapper _mapper;
        public CouponRepository(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public async Task<CouponDto> GetCouponByCode(string code)
        {
            var couponExist = await _dbContext.Coupons.FirstOrDefaultAsync(item => item.CounponCode == code);
            return _mapper.Map<CouponDto>(couponExist);
        }
    }
}
