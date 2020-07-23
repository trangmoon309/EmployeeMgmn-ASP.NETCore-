using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Identity.Models;
using Identity.ViewModels;
using Identity.ViewModels.Identity;
using Identity.Views.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

namespace Identity.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<AdministrationController> logger;

        public AdministrationController(RoleManager<IdentityRole> roleManager, 
            UserManager<ApplicationUser> userManager,
            ILogger<AdministrationController> logger)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.logger = logger;

        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }



        [HttpGet]
        [Authorize(Policy = "CreateRolePolicy")]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole role = new IdentityRole()
                {
                    Name = model.RoleName
                };
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRole", "Administration");
                }
                foreach (IdentityError err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }
            return View(model);
        }


        [HttpGet]
        //[Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> EditRole(string Id)
        {
            IdentityRole role = await roleManager.FindByIdAsync(Id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {Id} is Not Found!";
                return View("NotFound");
            }

            EditRoleViewModel eRole = new EditRoleViewModel()
            {
                Id = role.Id,
                RoleName = role.Name,
            };

            foreach (var user in userManager.Users.ToList())
            {
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    eRole.Users.Add(user.UserName);
                }
            }
            return View(eRole);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {model.Id} is Not Found!";
                return View("NotFound");
            }
            else
            {
                role.Name = model.RoleName;
                var result = await roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRole");
                }

                foreach (IdentityError err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }

            return View(model);
        }

        //User nào có Role = role đang cần tìm thì sẽ đc check (IsSelected = true)
        [HttpGet]
        public async Task<IActionResult> EditUserInSameRole(string roleId)
        {
            ViewBag.RoleId = roleId;
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id= {roleId} is not existed!";
                return View("NotFound");
            }

            var model = new List<UserRoleVIewModel>();
            foreach (var user in userManager.Users.ToList())
            {
                var userRoleViewModel = new UserRoleVIewModel()
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                };

                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.IsSeleted = true;
                }
                else userRoleViewModel.IsSeleted = false;

                model.Add(userRoleViewModel);

            }
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditUserInSameRole(List<UserRoleVIewModel> model, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

            for (int i = 0; i < model.Count; i++)
            {
                var user = await userManager.FindByIdAsync(model[i].UserId);

                IdentityResult result = null;

                //Nếu user được chọn và nó chưa có role này thì ta sẽ thêm role vào cho user
                if (model[i].IsSeleted && !(await userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }
                //Nếu user k đc chọn để thêm, mà nó đã có role này thì tức là ta xóa role này cho user
                else if (!model[i].IsSeleted && await userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                //Nếu user không được chọn và cũng không nằm trong role đó thì t k cần làm gì
                //Nếu user được chọn và nó cũng đã nằm trong role đó thì t k cần làm gì
                else
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                        continue;
                    else
                        return RedirectToAction("EditRole", new { Id = roleId });
                }
            }

            return RedirectToAction("EditRole", new { Id = roleId });
        }

        //Khi bạn là user nhưng bạn lại cố vào 1 action dành cho admin thì sẽ lỗi, và url sẽ tự động dẫn đến AccessDenied nên
        // bây giờ hãy tạo ra action đó đễ in ra một vài nội dung thông báo lỗi!
        // Ở bên AccoutController


        [HttpGet]
        public IActionResult ListRole()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }


        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = userManager.Users;
            return View(users);
        }


        [HttpGet]
        public async Task<IActionResult> EditUser(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = "This user is not Exist!";
                return View("NotFound");
            }
            var userClaims = await userManager.GetClaimsAsync(user);
            var userRoles = await userManager.GetRolesAsync(user);

            EditUserViewModel eUser = new EditUserViewModel()
            {
                 Id = user.Id,
                 UserName = user.Email,
                 Email = user.Email,
                 City = user.City,
                 Roles = userRoles,
                 Claims = userClaims.Select(c => c.Type + ": " + c.Value).ToList()
            };
            return View(eUser);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                ViewBag.ErrorMessage = "This user is not Exist!";
                return View("NotFound");
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.City = model.City;

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("ListUsers","Administration");
            }


            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userName)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                ViewBag.ErrorMessage = "This user is not Exist!";
                return View("NotFound");
            }

            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("ListUsers", "Administration");
            }


            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("EditUser");
        }


        [HttpPost]
       // [Authorize(Policy = "DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ViewBag.ErrorMessage = "This role is not Exist!";
                return View("NotFound");
            }

            try
            {
                var result = await roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRole", "Administration");
                }


                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }catch(Exception ex)
            {
                //Log the exception to a file. We discussed logging to a file
                // using Nlog in Part 63 of ASP.NET Core tutorial
                logger.LogError($"Exception Occured : {ex}");
                // Pass the ErrorTitle and ErrorMessage that you want to show to
                // the user using ViewBag. The Error view retrieves this data
                // from the ViewBag and displays to the user.
                ViewBag.ErrorTitle = $"{role.Name} role is in use";
                ViewBag.ErrorMessage = $"{role.Name} role cannot be deleted as there are users in this role. If you want to delete this role, please remove the users from the role and then try to delete";
                return View("Error");
            }


            return View("EditRole");
        }



        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRole(string userId)
        {
            ViewBag.userId = userId;
            var user = await userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return View("NotFound");
            }

            List<UserRoleVIewModel> model = new List<UserRoleVIewModel>();

            foreach(var role in roleManager.Roles.ToList())
            {
                var userRole = new UserRoleVIewModel()
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    UserId = userId,
                    UserName = user.UserName
                };
                //if (await userManager.IsInRoleAsync(user, role.Name))
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRole.IsSeleted = true;
                }
                else
                {
                    userRole.IsSeleted = false;
                }
                model.Add(userRole);
            }

            return View(model);
        }


        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRole(List<UserRoleVIewModel> model, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("NotFound");
            }


            var roles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user,roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing role.");
                return View(model);
            }

            result = await userManager.AddToRolesAsync(user, 
                    from item in model
                    where item.IsSeleted == true
                    select item.RoleName
                );

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add user existing role.");
                return View(model);
            }

            return RedirectToAction("EditUser", new { userId = userId });
        }



        [HttpGet]
        public async Task<IActionResult> ManageClaim(string userId)
        {
            var id = userId;
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return View("NotFound");
            }

            var existingClaims = await userManager.GetClaimsAsync(user);
            //Các claims dành cho user này
            var model = new UserClaimsViewModel()
            {
                UserId = userId,
            };

            foreach (Claim claim in ClaimsStore.AllClaims)
            {
                UserClaim userClaim = new UserClaim
                {
                    ClaimType = claim.Type
                };
                if (existingClaims.Any(c => c.Type == claim.Type && c.Value == "true"))
                {
                    userClaim.IsSelected = true;
                }

                model.Claims.Add(userClaim);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageClaim(UserClaimsViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return View("NotFound");
            }

            var claims = await userManager.GetClaimsAsync(user);
            var result = await userManager.RemoveClaimsAsync(user, claims);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing claims");
                return View(model);
            }

            result = await userManager.AddClaimsAsync(user,
                    from item in model.Claims
                    where item.IsSelected == true
                    select new Claim(item.ClaimType, item.IsSelected ? "true" : "false")
                );
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected claims to user");
                return View(model);
            }
            return RedirectToAction("EditUser", new { userId = model.UserId });
        }


    }
}
