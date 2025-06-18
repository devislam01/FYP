namespace DemoFYP.Models.Dto.Request
{
    public class UserLoginRequest
    {
        public string Email {  get; set; }
        public string Password { get; set; }
    }

    public class UserLogoutRequest
    {
        public string RefreshToken { get; set; }
    }
}
