using System.ComponentModel.DataAnnotations;

namespace Mango.Services.ProductAPI.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Range(0, 1000)]
        public double Price { get; set; }
        public string Description { get; set; }  = string.Empty;
        public int CategoryName { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}
