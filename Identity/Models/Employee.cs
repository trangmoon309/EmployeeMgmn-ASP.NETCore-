using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Models
{
    public class Employee 
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please supply your Name")]
        [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please supply your Email")]
        [Display(Name = "Offical Email")]
        [EmailAddress(ErrorMessage = "Please supply exact Email")]
        public string Email { get; set; }

        [Required]
        public Dept? Department { get; set; }
        public string PhotoPath { get; set; }
    }
}
