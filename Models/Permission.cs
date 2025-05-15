using Microsoft.AspNetCore.Http.HttpResults;

namespace DemoFYP.Models
{
    public class Permission
    {
        public int PermissionID { get; set; }
        public string PermissionName { get; set; } = null!;
        public string? PermissionDescription { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
