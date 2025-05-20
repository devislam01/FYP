using DemoFYP.Enums;

namespace DemoFYP.Models.Dto.Request
{
    public class PaymentListFilterRequest : PaginationRequest
    {
        public int? OrderId { get; set; }
        public double? TotalPaidAmount { get; set; }
        public int? PaymentMethodID { get; set; }
        public PaymentStatus? Status { get; set; }
        public DateTime? CreatedDateTime { get; set; }
    }

    public class UpdatePaymentRequest
    {
        public int PaymentId { get; set; }
        public double? TotalPaidAmount { get; set; }
        public int? PaymentMethodID { get; set; }
        public IFormFile? ReceiptFile { get; set; }
        public string? Receipt { get; set; }
        public string? Status { get; set; }
    }
}
