using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PharmaSphere.Domain.Entities;
using PharmaSphere.Domain.Enums;

namespace PharmaSphere.Infrastructure.Identity
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            IServiceProvider services)
        {
            var roleManager =
                services.GetRequiredService<
                    RoleManager<IdentityRole>>();

            var userManager =
                services.GetRequiredService<
                    UserManager<ApplicationUser>>();


            // =================================================
            // Roles
            // =================================================

            string[] roles =
            {
                "Admin",
                "Pharmacist",
                "Delivery",
                "Client"
            };


            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }


            // =================================================
            // Admin
            // =================================================

            const string adminEmail =
                "admin@pharmasphere.com";

            const string adminPassword =
                "Admin@123";


            var admin =
                await userManager.FindByEmailAsync(
                    adminEmail);


            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,

                    Email = adminEmail,

                    EmailConfirmed = true,

                    FullName = "System Administrator",

                    Role = UserRole.Admin,

                    CreatedAt = DateTime.UtcNow
                };


                var result =
                    await userManager.CreateAsync(
                        admin,
                        adminPassword);


                if (!result.Succeeded)
                {
                    throw new Exception(
                        string.Join(
                            ", ",
                            result.Errors.Select(
                                e => e.Description)));
                }
            }


            if (!await userManager.IsInRoleAsync(
                    admin,
                    "Admin"))
            {
                await userManager.AddToRoleAsync(
                    admin,
                    "Admin");
            }
        }
    }
}