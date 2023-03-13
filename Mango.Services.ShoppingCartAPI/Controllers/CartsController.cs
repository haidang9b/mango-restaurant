using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.Messages;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        private readonly IMessageBus _messageBus;
        protected ResponseDto _response;

        public CartsController(ICartRepository cartRepository, IMessageBus messageBus)
        {
            _cartRepository = cartRepository;
            _messageBus = messageBus;
            _response = new ResponseDto();
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<object> GetCart(string userId)
        {
            try
            {
                _response.Result = await _cartRepository.GetCartByUserId(userId);
            }
            catch(Exception ex)
            {
                _response.HandleException(ex);
            }
            return _response;
        }

        [HttpPost("AddCart")]
        public async Task<object> AddCart(CartDto cartDto)
        {
            try
            {
                _response.Result = await _cartRepository.CreateUpdateCart(cartDto);
            }
            catch (Exception ex)
            {
                _response.HandleException(ex);
            }
            return _response;
        }

        [HttpPut("UpdateCart")]
        public async Task<object> UpdateCart(CartDto cartDto)
        {
            try
            {
                _response.Result = await _cartRepository.CreateUpdateCart(cartDto);
            }
            catch (Exception ex)
            {
                _response.HandleException(ex);
            }
            return _response;
        }

        [HttpDelete("RemoveCart/{cartDetailsId}")]
        public async Task<object> RemoveCart(int cartDetailsId)
        {
            try
            {
                _response.Result = await _cartRepository.RemoveFromCart(cartDetailsId);
            }
            catch (Exception ex)
            {
                _response.HandleException(ex);
            }
            return _response;
        }

        [HttpPost("ApplyCoupon")]
        public async Task<object> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                _response.Result = await _cartRepository.ApplyCoupon(cartDto.CartHeader.UserId, 
                                    cartDto.CartHeader.CouponCode);
            }
            catch (Exception ex)
            {
                _response.HandleException(ex);
            }
            return _response;
        }

        [HttpPost("RemoveCoupon")]
        public async Task<object> RemoveCoupon([FromBody] string userId)
        {
            try
            {
                _response.Result = await _cartRepository.RemoveCoupon(userId);
            }
            catch (Exception ex)
            {
                _response.HandleException(ex);
            }
            return _response;
        }

        [HttpPost("Checkout")]
        public async Task<object> Checkout(CheckoutHeaderDto checkoutHeader)
        {
            try
            {
                CartDto cartDto = await _cartRepository.GetCartByUserId(checkoutHeader.UserId);

                if(cartDto == null)
                {
                    return BadRequest();
                }

                checkoutHeader.CartDetails = cartDto.CartDetails;

                // logic to add message to process order
                await _messageBus.PublishMessage(checkoutHeader, "checkoutmessagetopic");
               
            }
            catch (Exception ex)
            {
                _response.HandleException(ex);
            }
            return _response;
        }
    }
}
