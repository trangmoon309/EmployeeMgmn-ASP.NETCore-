using Identity.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.ViewModels.Home
{
    public class EmployeeCreateViewModel
    {
        [Required(ErrorMessage = "Please supply your Name")]
        [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please supply your Email")]
        [Display(Name = "Offical Email")]
        [EmailAddress(ErrorMessage = "Please supply exact Email")]
        public string Email { get; set; }

        [Required]
        public Dept? Department { get; set; }

        //IFormFile là 1 interface, chúng ta sẽ dùng thuộc tính FileName và method CopyTo để copy hình ảnh đến một folder lưu hình ảnh trên
        // webserver
        // Tại sao chúng ta phải tạo ra 1 class View cho employeecreate: Vì để có thể truy cập uploaded file trên server thông qua
        // model binding. Chúng ta cũng có thể dễ dàng thay đổi kiểu dữ liệu từ string sang IFormFile bên Employee.cs nhưng, nó là một đối tượng
        //  phức tạp và khi ta tạo navigation property thì nó cũng sẽ rất phức tạp. Ngoài ra, ở dưới Database, ta chỉ muốn lưu FileName(đường dẫn)
        // của image thôi chứ k cần các thuộc tính kia nên t k cần phải để kiểu IFormFile bên Employees.cs
        // Và vì vậy, để hiện thị đầy đủ thông tin của image trên View + với việc để chúng ta có thể nhận được các file được uploaded trên server
        // và truy cập vào nó thông qua model binding
        public List<IFormFile> Photo { get; set; }
    }
}
