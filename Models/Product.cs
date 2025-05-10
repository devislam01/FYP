using System;
using System.Collections.Generic;

namespace DemoFYP;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string ProductDescription { get; set; } = null!;

    public int CategoryId { get; set; }

    public string ProductCondition { get; set; } = null!;

    public string ProductImage { get; set; } = null!;

    public double ProductPrice { get; set; }

    public double? ProductRating { get; set; }

    public int StockQty { get; set; } = 1;

    public DateTime CreatedDateTime { get; set; }

    public DateTime? UpdatedDateTime { get; set; }

    public DateTime? DeletedDateTime { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public Guid? DeletedBy { get; set; }

    public sbyte IsActive { get; set; } = 1;

    public Guid UserId { get; set; }

    public virtual ProductCategory Category { get; set; } = null!;
}
