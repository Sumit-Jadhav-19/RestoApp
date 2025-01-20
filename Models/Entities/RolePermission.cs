using Microsoft.AspNetCore.Identity;

namespace RestoApp.Models.Entities
{
    public class UserPermission
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Foreign key to AspNetRoles
        public ApplicationUser User { get; set; }

        public int PermissionId { get; set; } // Foreign key to Permissions
        public Permission Permission { get; set; }
    }
}
