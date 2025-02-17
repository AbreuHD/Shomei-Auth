using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Enums;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Seeds
{
    public static class DefaultUser
    {
        public static async Task Seed(UserManager<ApplicationUser> userManager)
        {
            ApplicationUser user = new()
            {
                Name = "Jefferson",
                LastName = "Abreu",
                UserName = "AbreuHDm",
                Email = "abreuhd08@gmail.com",
                PhoneNumber = "809-491-0570",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true

            };
            if (userManager.Users.All(u => u.Id != user.Id))
            {
                var userEmail = await userManager.FindByEmailAsync(user.Email);
                if (userEmail == null)
                {
                    await userManager.CreateAsync(user, "123Pa$$word!");
                    await userManager.AddToRoleAsync(user, Roles.User.ToString());
                }

            }
        }
    }
}
