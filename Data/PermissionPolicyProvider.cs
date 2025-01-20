using Microsoft.AspNetCore.Authorization;

namespace RestoApp.Data
{
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        const string PolicyPrefix = "Permission_";

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var permissionName = policyName.Substring(PolicyPrefix.Length);
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new PermissionRequirement(permissionName))
                    .Build();

                return Task.FromResult(policy);
            }

            return Task.FromResult<AuthorizationPolicy>(null);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return Task.FromResult(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return Task.FromResult<AuthorizationPolicy?>(null);
        }
    }
}
