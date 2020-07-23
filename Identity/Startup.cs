using Identity.Models;
using Identity.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Identity
{
    public class Startup
    {
        private IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddDbContextPool<AppDbContext>(options => options.UseSqlServer(_configuration.GetConnectionString("EmployeeDBConnection")));

            //Authentication Service
            services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                options.Password.RequiredLength = 10;
                options.Password.RequiredUniqueChars = 3;
                options.Password.RequireNonAlphanumeric = false;
            })
                    .AddEntityFrameworkStores<AppDbContext>();

            //services.Configure<IdentityOptions>(options =>
            //{
            //    options.Password.RequiredLength = 10;
            //    options.Password.RequiredUniqueChars = 3;
            //    options.Password.RequireNonAlphanumeric = false;
            //});


            //Authorization Service : Require authenticated users
            services.AddMvc(option => {
                var policy = new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser() //yêu cầu người dùng đã được xác thực auth
                                .Build(); //chạy các yêu cầu đã được nếu ở trên
                option.Filters.Add(new AuthorizeFilter(policy));

            }).AddXmlDataContractSerializerFormatters();

            //Authentication
            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = "711032757060-s6jjl37ojld6hesklhnf8k2e0uq1gf32.apps.googleusercontent.com";
                    options.ClientSecret = "dq-NVT8sVxs6Fq2fITZ1BAZw";
                })
                .AddFacebook(options =>
                {
                    options.AppId = "739539140204849";
                    options.AppSecret = "845b5eb7d87539efd5fe5970c016095d";
                });



            //AccessDenied
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });


            //Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("DeleteRolePolicy", policy => policy.RequireClaim("Delete Role"));

                //Yêu cầu Edit Role của người này phải có claim value = true thì mới thực hiện đư

                //Cách này không dùng Func
                //options.AddPolicy("EditRolePolicy", policy => policy
                //   .RequireRole("Admin")
                //   .RequireClaim("Edit Role", "true")
                //);

                //Cách này dùng Func
                //options.AddPolicy("EditRolePolicy", policy => policy.RequireAssertion(context => 
                //    context.User.IsInRole("Admin") &&
                //    context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                //    context.User.IsInRole("Super Admin")
                //));

                //Cách này dùng Func nhưng các điều kiện được tách ra ở một hàm riêng để dễ quản lí
                //options.AddPolicy("EditRolePolicy", policy => policy.RequireAssertion(context => 
                //AuthorizeAccessToEditRole(context)
                //));

                //Cách này custom = cách tạo ra class mới implement lại AuthorizationRequiment để handle các requiment 
                //Tham số thứ nhất là AuthorizationRequiment
                options.AddPolicy("EditRolePolicy", policy =>
                        policy.AddRequirements(new ManageAdminRoleAndClaimRequirement()));

                options.AddPolicy("CreateRolePolicy", policy => policy.RequireClaim("Create Role"));


                options.AddPolicy("AdminRolePolicy", policy => policy.RequireRole("Admin"));
                //Từ đây, ta có thể dùng:
                //C1: [Authorize(Roles="Admin")
                //C2: [Authorize(Policy = "AdminRolePolicy")]


                //Sau khi có  1 handler trả về Fail thì sẽ ngưng gọi các handlers khác
                //options.InvokeHandlersAfterFailure = false;

            }); //Có nghĩa là Policy này có tên DeleteRolePolicy, và các controller/action nào dùng policy này
            //thì có nghĩa là để có thể thực thi đc controller/action đó thì cần phải có RequireClaim là Delete Role
            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();
            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error");
                //app.UseHsts();
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }


        public bool AuthorizeAccessToEditRole(AuthorizationHandlerContext context)
        {
            return context.User.IsInRole("Admin") &&
                    context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                    context.User.IsInRole("Super Admin");
        }
    }
}
