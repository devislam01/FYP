namespace DemoFYP.Models.Dto.Request
{
    public class UserRegisterRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int? RoleID {  get; set; }
    }
}
