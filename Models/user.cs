using System;
using System.Collections.Generic;

namespace DemoFYP;

public partial class User
{
    public Guid UserId { get; set; }

    public int UserLevel { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? UserName { get; set; }

    public string? UserGender { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public double? RatingMark { get; set; }

    public sbyte IsActive { get; set; }

    public DateTime CreatedDateTime { get; set; }

    public DateTime? UpdatedDateTime { get; set; }

    public DateTime? DeletedDateTime { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public Guid? DeletedBy { get; set; }
}
