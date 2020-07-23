using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Identity.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Identity.ViewModels;
using Identity.ViewModels.Home;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace Identity.Controllers
{
    public class HomeController : Controller
    {

        private IEmployeeRepository _employeeRepository;
        private IHostingEnvironment hostingEnvironment;
        public HomeController(IEmployeeRepository employeeRepository, IHostingEnvironment hostingEnvironment)
        {
            _employeeRepository = employeeRepository;
            this.hostingEnvironment = hostingEnvironment;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var model = _employeeRepository.GetAllEmployee();
            return View(model);
        }


        [Route("Home/Details/{id}")]
        [AllowAnonymous]
        public IActionResult ViewDetail(int id)
        {

            Employee employee = _employeeRepository.GetEmployee(id);
            if (employee == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", id);
            }
            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Employee = _employeeRepository.GetEmployee(id),
                PageTitle = "Employee Details"
            };
            return View(homeDetailsViewModel);
        }


        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                //FILE UPLOAD(Lesson 53)
                string uniqueFileName = null;
                if (model.Photo != null && model.Photo.Count > 0)
                {
                    foreach (IFormFile photo in model.Photo)
                    {
                        //hàm combine trả về path(string) đến folder images trong wwwroot
                        string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                        //nếu 2 hình ảnh giống nhau, ta làm thành 1 fileName duy nhất (unique FileName)
                        //uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                        uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        //Dùng copyto để copy file đc upload vào images folder
                        //model.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
                        photo.CopyTo(new FileStream(filePath, FileMode.Create));
                    }

                }
                Employee newE = new Employee()
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    PhotoPath = uniqueFileName
                };

                _employeeRepository.AddEmployee(newE);
                //Sau khi thêm thì ta muốn viết lại chi tiết về user mới đc thêm, vì vậy ta có thể xem được chi tiết những thứ được thêm vào
                // Vì vậy ta return RedirectToAction
                return RedirectToAction("details", new { id = newE.Id });
            }
            return View();
        }


        [HttpGet]
        [Authorize]
        public ActionResult Edit(int id)
        {
            Employee e = _employeeRepository.GetEmployee(id);
            EmployeeEditViewModel ed = new EmployeeEditViewModel()
            {
                Id = e.Id,
                Email = e.Email,
                Name = e.Name,
                Department = e.Department,
                ExistingPhotoGraph = e.PhotoPath
            };
            return View(ed);
        }

        [HttpPost]
        [Authorize]
        public ActionResult Edit(EmployeeEditViewModel e)
        {
            if (ModelState.IsValid)
            {
                Employee ed = _employeeRepository.GetEmployee(e.Id);
                ed.Name = e.Name;
                ed.Email = e.Email;
                ed.Department = e.Department;
                string uniqueFileName = ProcessUploadedFile(e);
                //Nếu người dùng chọn ảnh thay thế
                if (e.Photo != null)
                {
                    if (e.ExistingPhotoGraph != null)
                    {
                        string filePath = Path.Combine(hostingEnvironment.WebRootPath, "images", e.ExistingPhotoGraph);
                        System.IO.File.Delete(filePath);
                    }
                    ed.PhotoPath = ProcessUploadedFile(e);
                }

                _employeeRepository.Update(ed);
                return RedirectToAction("details", new { id = ed.Id });

            }

            return View();
        }

        private string ProcessUploadedFile(EmployeeCreateViewModel model)
        {
            string uniqueFileName = null;
            if (model.Photo != null)
            {
                //hàm combine trả về path(string) đến folder images trong wwwroot
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                //nếu 2 hình ảnh giống nhau, ta làm thành 1 fileName duy nhất (unique FileName)
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo[0].FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                //Dùng copyto để copy file đc upload vào images folder
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Photo[0].CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
    }
}
