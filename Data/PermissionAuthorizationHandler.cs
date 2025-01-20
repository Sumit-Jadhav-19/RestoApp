using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestoApp.Models.Entities;
using System.Security.Claims;

namespace RestoApp.Data
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public PermissionAuthorizationHandler(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return;

            // Find user by userId
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;  // Handle case when user is not found

            // Proceed with your logic to retrieve roles/permissions
            var rolePermissions = await _dbContext.UserPermissions
                .Where(rp => rp.UserId == user.Id)
                .Select(rp => rp.Permission.Name)
                .ToListAsync();

            // Check if the required permission is in the list
            if (rolePermissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
    }
}
