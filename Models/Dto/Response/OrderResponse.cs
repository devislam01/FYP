namespace DemoFYP.Models.Dto.Response
{
    public class ProceedToPaymentResponse
    {
        public int PaymentID {  get; set; }
        public int OrderID {  get; set; }
    }

    #region Admin Dto

    public class OrderListResponse
    {
        public int OrderID { get; set; }
        public Guid UserID { get; set; }
        public int PaymentID { get; set; }
        public double TotalAmount {  get; set; }
        public string Status {  get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion
}
