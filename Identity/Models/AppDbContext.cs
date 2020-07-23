using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Models
{
    //Set ApplicationUser để có thể add-migration để sinh code tạo ra column mới mà ta vừa thêm
    //Nếu để IdentityDbContext thôi thì khi ta add-migartion, ở file Up() và Down() sẽ k tự động sinh code
    //Vì nó đang sinh code dựa trên IdentityDbContext<IdentityUser> => nó k chứa column mới mà ta thêm vào => k sinh code
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Employee> employees { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Seed();

            /*Xóa chế độ cascade cho bảng roleuser */
            foreach(var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

    }
}
