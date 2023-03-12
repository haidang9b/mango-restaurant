namespace Mango.Services.ShoppingCartAPI.Models.Dto
{
    public class CartDto
    {
        public CartHeaderDto CarHeader { get; set; }
        public IEnumerable<CartDetailsDto> CartDetails { get; set; } = Enumerable.Empty<CartDetailsDto>();
    }
}
