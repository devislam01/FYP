namespace DemoFYP.Models.Dto.Request
{
    public class AddRoleRequest
    {
        public string RoleName {  get; set; }
        public bool? IsActive { get; set; } = false;
    }
}
