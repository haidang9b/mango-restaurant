namespace Mango.Services.ShoppingCartAPI.Models
{
    public class Cart
    {
        public CartHeader CarHeader { get; set; }
        public IEnumerable<CartDetails> CartDetails { get; set; } = Enumerable.Empty<CartDetails>();
    }
}
