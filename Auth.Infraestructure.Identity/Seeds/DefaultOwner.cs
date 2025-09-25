using Microsoft.AspNetCore.Identity;
using Shomei.Infraestructure.Identity.Entities;

namespace Shomei.Infraestructure.Identity.Seeds
{
    public static class DefaultOwner
    {
        public static async Task Seed(UserManager<ApplicationUser> userManager)
        {
            var user = new ApplicationUser
            {
                Name = "John",
                LastName = "Doe",
                UserName = "Owner",
                Email = "John@Doe.com",
                EmailConfirmed = true
            };
            if (userManager.Users.All(u => u.Id != user.Id))
            {
                var userEmail = await userManager.FindByEmailAsync(user.Email);
                if (userEmail == null)
                {
                    await userManager.CreateAsync(user, "123Pa$$word!");
                    await userManager.AddToRoleAsync(user, "Owner");
                }
            }
        }
    }
}
