namespace DemoFYP.Models.Dto.Response
{
    public class UserDetailResponse
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string UserGender { get; set; }

        public string Address { get; set; }
    }

    public class UserJwtClaims
    {
        public Guid UserID { get; set; }
        public string Role {  get; set; }
        public string Email { get; set; }
        public List<string> Permissions { get; set; }
    }

    public class UserPermissionResponse
    {
        public List<string> Permissions { get; set; }
    }
}
