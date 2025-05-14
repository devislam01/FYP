namespace DemoFYP.Models.Dto.Request
{
    public class UserUpdateDetailRequest
    {
        public Guid? UserID { get; set; }

        public string? UserName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? UserGender { get; set; }

        public string? Address { get; set; }
    }

    public class RevokeUserRequest
    {
        public Guid UserID { get; set; }
    }
}
