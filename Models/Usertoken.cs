using System;
using System.Collections.Generic;

namespace DemoFYP;

public partial class Usertoken
{
    public int TokenId { get; set; }

    public Guid UserId { get; set; }

    public string AccessToken { get; set; } = null!;

    public DateTime AccessTokenExpiresAt { get; set; }

    public string RefreshToken { get; set; } = null!;

    public DateTime RefreshTokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
