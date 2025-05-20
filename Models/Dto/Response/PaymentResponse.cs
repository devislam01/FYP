namespace DemoFYP.Models.Dto.Response
{
    public class PaymentListResponse
    {
        public int PaymentID {  get; set; }
        public int OrderID {  get; set; }
        public double TotalPaidAmount { get; set; }
        public int PaymentMethodID {  get; set; }
        public string? Receipt {  get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
