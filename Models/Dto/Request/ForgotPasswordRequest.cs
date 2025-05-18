namespace DemoFYP.Models.Dto.Request
{
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Password { get; set; } = null!;
    }

    public class AdminResetPasswordRequest
    {
        public Guid UserID { get; set; }
        public string Password { get; set; } = null!;
    }
}
