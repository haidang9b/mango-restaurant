using Mango.Services.ShoppingCartAPI.Models.Dto;
using Newtonsoft.Json;

namespace Mango.Services.ShoppingCartAPI.Repositories
{
    public class CouponRepository : ICouponRepository
    {
        private readonly HttpClient _client;
        public CouponRepository(HttpClient client)
        {
            _client = client;
        }
        public async Task<CouponDto> GetCoupon(string couponCode)
        {
            var response = await _client.GetAsync($"/api/coupon/{couponCode}");
            var apiContent = await response.Content.ReadAsStringAsync();
            var responseObj = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
            if(responseObj.IsSuccess)
            {
                return JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(responseObj.Result));
            }
            return new CouponDto();
        }
    }
}
