using RestoApp.Models.Entities;

namespace RestoApp.Models.ViewModels
{
    public class RolePermissionViewModel
    {
        public int RoleId { get; set; } // Primary Key for Role
        public string Role { get; set; } // Role Name
        public List<PermissionViewModel> Permissions { get; set; }
    }
    public class PermissionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g., "ViewReports"
        public string Description { get; set; } // Optional
        public bool HasPermission { get; set; }
    }
    public class RoleAndPermissionsViewModel
    {
        public List<UserPermission> UserPermissions { get; set; }
        public List<PermissionViewModel> permissionViewModels { get; set; }
        public List<RolePermissionViewModel> RolePermissionViewModel { get; set; }
    }

}
