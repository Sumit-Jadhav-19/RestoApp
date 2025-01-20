using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestoApp.Models.Entities;
using System.Reflection.Emit;

namespace RestoApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Hall> Halls { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<OrderMaster> OrderMasters { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<UserConnection> UserConnections { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<UserPermission>()
                .HasOne(rp => rp.User)
                .WithMany()
                .HasForeignKey(rp => rp.UserId);

            builder.Entity<UserPermission>()
                .HasOne(rp => rp.Permission)
                .WithMany()
                .HasForeignKey(rp => rp.PermissionId);

            builder.Entity<Menu>()
                .HasOne(m => m.Category)
                .WithMany()
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Table>()
                .HasOne(m => m.Hall)
                .WithMany()
                .HasForeignKey(m => m.HallId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
