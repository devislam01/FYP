using System;
using System.Collections.Generic;

namespace DemoFYP;

public partial class Payment
{
    public Guid PaymentId { get; set; }

    public Guid OrderId { get; set; }

    public double TotalPaidAmount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedDateTime { get; set; }

    public DateTime UpdatedDateTime { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid UpdatedBy { get; set; }
}
