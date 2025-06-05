using DemoFYP.Models;
using System;
using System.Collections.Generic;

namespace DemoFYP;

public partial class Order
{
    public int OrderId { get; set; }

    public Guid UserId { get; set; }

    public int? PaymentId { get; set; }

    public double TotalAmount { get; set; }

    public double? Rating { get; set; }

    public string? Feedback { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? CancelReason {  get; set; }

    public DateTime CreatedDateTime { get; set; }

    public DateTime? UpdatedDateTime { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public List<OrderItems> OrderItems { get; set; } = new();
    public ICollection<Payment> Payment { get; set; } = [];
}
