using DemoFYP.Models;

namespace DemoFYP;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public double TotalPaidAmount { get; set; }

    public int PaymentMethodID {  get; set; }

    public Guid SellerID { get; set; }

    public string? Receipt {  get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedDateTime { get; set; }

    public DateTime? UpdatedDateTime { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public Order Order { get; set; } = null!;
    public PaymentMethod PaymentMethod { get; set; } = null!;
}
