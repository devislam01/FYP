using System;
using System.Collections.Generic;

namespace DemoFYP;

public partial class Admin
{
    public byte[] UserId { get; set; } = null!;

    public int UserLevel { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime CreatedDateTime { get; set; }

    public DateTime UpdatedDateTime { get; set; }

    public DateTime DeletedDateTime { get; set; }

    public byte[] CreatedBy { get; set; } = null!;

    public byte[] UpdatedBy { get; set; } = null!;

    public byte[] DeletedBy { get; set; } = null!;
}
