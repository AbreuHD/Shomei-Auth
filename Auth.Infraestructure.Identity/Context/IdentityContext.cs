using Auth.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Auth.Infraestructure.Identity.Context
{
    public class IdentityContext : IdentityDbContext<ApplicationUser>
    {
        public IdentityContext(DbContextOptions<IdentityContext> option) : base(option) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //modelBuilder.HasDefaultSchema("Identity");
            builder.Entity<ApplicationUser>(entity => entity.ToTable("Users"));
            builder.Entity<IdentityRole>(entity => entity.ToTable("Roles"));
            builder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("UserRoles"));
            builder.Entity<UserProfile>(entity => entity.ToTable("UserProfile"));

            builder.Entity<UserProfile>()
               .HasOne(p => p.User)
               .WithMany(u => u.UserProfile)
               .HasForeignKey(p => p.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
