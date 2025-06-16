namespace DemoFYP.Models
{
    public class OrderItems
    {
        public int OrderItemID { get; set; }
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public double UnitPrice { get; set; }
        public int Qty { get; set; }
        public double Subtotal { get; set; }
        public string Status { get; set; } = null!;
        public string? CancelReason {  get; set; }
        public string? RejectReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }

        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
