using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Models
{
    public class SellerReview
    {
        public int ReviewID { get; set; }
        public int OrderItemID { get; set; }
        public Guid SellerID { get; set; }
        public Guid BuyerID { get; set; }

        public double Rating { get; set; }
        public string? Feedback { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }

        public OrderItem OrderItem { get; set; } = null!;
    }
}
