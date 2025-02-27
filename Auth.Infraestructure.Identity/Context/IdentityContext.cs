using Auth.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infraestructure.Identity.Context
{
    public class IdentityContext(DbContextOptions<IdentityContext> option) : IdentityDbContext<ApplicationUser>(option)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //modelBuilder.HasDefaultSchema("Identity");
            builder.Entity<ApplicationUser>(entity => entity.ToTable("Users"));
            builder.Entity<IdentityRole>(entity => entity.ToTable("Roles"));
            builder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("UserRoles"));
            builder.Entity<UserProfile>(entity => entity.ToTable("UserProfile"));
            builder.Entity<MailOtp>(entity => entity.ToTable("MailOtp"));

            builder.Entity<UserProfile>()
                .HasOne(p => p.User)
                .WithMany(u => u.UserProfile)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserSession>()
                .HasOne(us => us.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MailOtp>()
                .HasOne(us => us.User)
                .WithMany(u => u.MailOtp)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
