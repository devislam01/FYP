using System;
using System.Collections.Generic;

namespace DemoFYP;

public partial class Order
{
    public byte[] OrderId { get; set; } = null!;

    public Guid UserId { get; set; }

    public byte[] ProductId { get; set; } = null!;

    public Guid PaymentId { get; set; }

    public int Quantity { get; set; }

    public double TotalAmount { get; set; }

    public int Rating { get; set; }

    public string Feedback { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedDateTime { get; set; }

    public DateTime UpdatedDateTime { get; set; }

    public DateTime DeletedDateTime { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid UpdatedBy { get; set; }

    public Guid DeletedBy { get; set; }
}
