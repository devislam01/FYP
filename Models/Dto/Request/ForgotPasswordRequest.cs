namespace DemoFYP.Models.Dto.Request
{
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
