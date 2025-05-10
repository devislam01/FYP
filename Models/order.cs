using System;
using System.Collections.Generic;

namespace DemoFYP;

public partial class Order
{
    public byte[] OrderId { get; set; } = null!;

    public byte[] UserId { get; set; } = null!;

    public byte[] ProductId { get; set; } = null!;

    public byte[] PaymentId { get; set; } = null!;

    public int Quantity { get; set; }

    public double TotalAmount { get; set; }

    public int Rating { get; set; }

    public string Feedback { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedDateTime { get; set; }

    public DateTime UpdatedDateTime { get; set; }

    public DateTime DeletedDateTime { get; set; }

    public byte[] CreatedBy { get; set; } = null!;

    public byte[] UpdatedBy { get; set; } = null!;

    public byte[] DeletedBy { get; set; } = null!;
}
