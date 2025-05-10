using System;
using System.Collections.Generic;

namespace DemoFYP;

public partial class Payment
{
    public byte[] PaymentId { get; set; } = null!;

    public byte[] OrderId { get; set; } = null!;

    public double TotalPaidAmount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedDateTime { get; set; }

    public DateTime UpdatedDateTime { get; set; }

    public byte[] CreatedBy { get; set; } = null!;

    public byte[] UpdatedBy { get; set; } = null!;
}
