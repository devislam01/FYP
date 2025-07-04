﻿using DemoFYP.Models;
using System;
using System.Collections.Generic;

namespace DemoFYP;

public partial class User
{
    public Guid UserId { get; set; }

    public int RoleID { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? UserName { get; set; }

    public string? UserGender { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }
    public string? ResidentialCollege {  get; set; }

    public double? RatingMark { get; set; }

    public string? PaymentQRCode {  get; set; }

    public string? Shopping_Cart {  get; set; }

    public sbyte IsActive { get; set; }

    public DateTime CreatedDateTime { get; set; }

    public DateTime? UpdatedDateTime { get; set; }

    public DateTime? DeletedDateTime { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public Guid? DeletedBy { get; set; }
    public virtual Role Role { get; set; } = null!;
}
