using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Models
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            //Khởi tạo các entity
            modelBuilder.Entity<Employee>().HasData(
                new Employee()
                {
                    Id = 1,
                    Name = "Mark",
                    Department = Dept.IT,
                    Email = "mark@gmail.com"
                },
                new Employee()
                {
                    Id = 3,
                    Name = "Deep",
                    Department = Dept.HR,
                    Email = "deep@gmail.com"
                }
            );
        }
    }
}
