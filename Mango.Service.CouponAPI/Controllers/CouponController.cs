using Mango.Service.CouponAPI.Models.Dtos;
using Mango.Service.CouponAPI.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Service.CouponAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private readonly ICouponRepository _couponRepository;
        protected ResponseDto _response;
        public CouponController(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
            _response = new();
        }

        [HttpGet("{code}")]
        public async Task<object> GetCouponByCode(string code)
        {
            try
            {
                _response.Result = await _couponRepository.GetCouponByCode(code);
            }
            catch(Exception ex)
            {
                _response.HandleException(ex);
            }
            return _response;
        }
    }
}
