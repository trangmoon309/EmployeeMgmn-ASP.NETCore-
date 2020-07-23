using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Identity.Security
{
    //<ột ADMIN có thể quản lí role và claim cho các Admin khác, nhưng họ k thể chỉnh sửa cho role và claim của chính họ
    public class CanEditOnlyOtherAdminRolesAndClaimsHandler :
        AuthorizationHandler<ManageAdminRoleAndClaimRequirement>
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CanEditOnlyOtherAdminRolesAndClaimsHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ManageAdminRoleAndClaimRequirement requirement)
        {
            //context.Resource trả về các action methods mà chúng ta đang muốn bảo vệ as AuthorizationFilterContext
            //Trong ví dụ của chúng ta đó là (ManageRole và ManageClaim) action methods
            if (context.User == null || !context.User.Identity.IsAuthenticated)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            //Vì NameIdentifier chứa ID của logged in user
            //Chúng ta muốn lấy ID của các admin sẽ được edit, sẽ phải pass qua URL với parameter
            //và chúng ta lấy giá trị tham số của query string này bằng cách dùng IHttpContextAccessor

            //NameIdentifier: {http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier: userId}
            string loggedInAdminId =
                context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            string adminIdBeingEdited = httpContextAccessor.HttpContext.Request.Query["userId"].ToString();

            if (context.User.IsInRole("Admin") &&
            context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") &&
            adminIdBeingEdited.ToLower() != loggedInAdminId.ToLower())
            {
                context.Succeed(requirement);
            }
            //Part 103
            /*
            else
            {
                context.Fail() ;
            } */

            return Task.CompletedTask;
        }
    }
}
