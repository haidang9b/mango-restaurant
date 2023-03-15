using System.ComponentModel.DataAnnotations;

namespace Mango.Service.CouponAPI.Models
{
    public class Coupon
    {
        [Key]
        public int CounponId { get; set; }
        public string CounponCode { get; set; }
        public double DiscountAmount { get; set; }
    }
}
