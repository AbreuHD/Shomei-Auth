using Microsoft.AspNetCore.Identity;

namespace Shomei.Infraestructure.Identity.Seeds
{
    public static class DefaultRoles
    {
        public static async Task Seed(RoleManager<IdentityRole> roleManager, IEnumerable<string> roles)
        {
            if (roles.Contains("Owner"))
            {
                roles = roles.Append("Owner");
            }
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
