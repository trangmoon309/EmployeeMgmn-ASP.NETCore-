using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Models
{
    public class SQLEmployeeRepository : IEmployeeRepository
    {

        private readonly AppDbContext context;
        public SQLEmployeeRepository(AppDbContext context)
        {
            this.context = context;
        }
        public Employee GetEmployee(int Id)
        {
            Employee e = context.employees.Find(Id);
            return e;
        }
        public IEnumerable<Employee> GetAllEmployee()
        {
            return context.employees;

        }
        public Employee AddEmployee(Employee e)
        {
            context.employees.Add(e);
            context.SaveChanges();
            return e;
        }
        public Employee Update(Employee eChanges)
        {
            var e = context.employees.Attach(eChanges);
            e.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return eChanges;
        }
        public Employee Delete(int Id)
        {
            Employee e = context.employees.Find(Id);
            if (e != null)
            {
                context.employees.Remove(e);
                context.SaveChanges();
            }
            return e;
        }
    }
}
