namespace DemoFYP.Models.Dto.Request
{
    public class PermissionRequest
    {
        public string PermissionName {  get; set; }
        public string PermissionDescription {  get; set; }
        public bool? IsActive { get; set; } = true;
    }

    public class BindRolePermissionRequest
    {
        public int RoleID { get; set; }
        public List<int> PermissionIDs { get; set; }
    }
}
