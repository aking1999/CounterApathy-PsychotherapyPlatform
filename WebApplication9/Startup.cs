using BackgroundTasks;
using Database.Data;
using Database.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Framework.Notifications;
using Framework.RouteMiddlewares;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace WebApplication9
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        //ako ne proradi, stavljam void da vraca i gore brisem .Wait()
        private async Task CreateClientRole(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            //var UserManager = serviceProvider.GetRequiredService<UserManager<CustomClient>>();

            //IdentityResult roleResult;

            //Adding Admin Role
            bool clientRoleExists = await roleManager.RoleExistsAsync("Client");

            if (!clientRoleExists)
            {
                //create the roles and seed them to the database
                await roleManager.CreateAsync(new IdentityRole("Client"));
            }

            //Assign Admin role to the main User here we have given our newly registered 
            //login id for Admin management
            //CustomClient user = await UserManager.FindByEmailAsync("v-nany@hotmail.com");
            //await UserManager.AddToRoleAsync(user, "Admin");
        }

        private async Task CreateTherapistRole(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            bool therapistRoleExists = await roleManager.RoleExistsAsync("Therapist");

            if (!therapistRoleExists)
            {
                await roleManager.CreateAsync(new IdentityRole("Therapist"));
            }
        }

        private async Task CreateAdminRole(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            bool adminRoleExists = await roleManager.RoleExistsAsync("Admin");

            if (!adminRoleExists)
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
        }

        private async Task CreateAdminUser(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<CustomClient>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher<CustomClient>>();

            //!!! ovo kasnije treba da zamenim da dodam normalan username, email i sifru.
            var admin = new CustomClient()
            {
                Id = "q_q777-vfkj645ghn-87dfjnbj-nc3y74tbQ_Q",
                UserName = "3@3",
                NormalizedUserName = "3@3",
                Email = "3@3",
                NormalizedEmail = "3@3",
                EmailConfirmed = true,
                SecurityStamp = "t_t999-734ybfo8egt-634gbgyby-eybrghfT_T",
                ConcurrencyStamp = "r_r111-vyd56r56uvy-674gbdf-78465gnR_R",
                PhoneNumber = "+381 631731222",
                PhoneNumberConfirmed = true,
                // !!! ovo isto kasnije da stavim na true
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                FirstName = "Admin",
                LastName = "[Main]",
                WebCredit = 1000000,
                YearOfBirth = 1999
            };

            //!!! ovo isto da promenim da stavim normalnu sifru
            var hashedPassword = passwordHasher.HashPassword(admin, "3");
            admin.PasswordHash = hashedPassword;

            var adminUser = await userManager.FindByIdAsync(admin.Id);

            if(adminUser == null)
            {
                await CreateAdminRole(serviceProvider);

                await userManager.CreateAsync(admin);
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddAuthorization(options => options.AddPolicy("Admin", policy => policy.RequireRole("Admin")));
            /*#region Repositories
            services.AddTransient<UnitOfWork, UnitOfWork>();
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            #endregion*/

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<CustomClient, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddDefaultUI()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //.AddTokenProvider<DataProtectorTokenProvider<CustomerUser>>(TokenOptions.DefaultProvider);
            //.AddRoles<IdentityRole>()

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                //ovo sam stavio da me ne baca na defaultnu login stranu areas/identity/admin/login kad nisam logovan, 
                //a unesem naziv rute za koju je potrebna autentifikacija
                options.LoginPath = new PathString("/Authorization/SignIn");
                //options.AccessDeniedPath = new PathString("/Therapists/All");
                //options.LogoutPath = new PathString("/[your-path]");
            });

            //services.AddControllersWithViews()
            //    .AddNewtonsoftJson(options =>
            //        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddSession();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            /* ovo mozda nije potrebno, probati kasnije bez ovoga*/
            //services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            //!!! ovde mozda bude greska. u ovom drugom PasswordHasher<CustomCLiernt> ili u AddScoped
            services.AddScoped<IPasswordHasher<CustomClient>, PasswordHasher<CustomClient>>();

            services.AddHostedService<TherapistSessionPaymentWorker>();
            services.AddHostedService<UpcomingSessionNotificationSenderWorker>();

            services.AddTransient<INotificationRepository, NotificationRepository>();
            services.AddSignalR();
            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            app.UseMiddleware<IgnoreRouteMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapGet("/{controller=base}/{action}", async (context) =>
                //{
                //    context.Response.
                //});

                endpoints.MapHub<NotificationHub>("/notificationHub");

                endpoints.MapAreaControllerRoute(
                    name: "Admin",
                    areaName: "Admin",
                    pattern: "Admin/{controller=Account}/{action=Profile}");

                endpoints.MapAreaControllerRoute(
                    name: "Therapist",
                    areaName: "Therapist",
                    pattern: "Therapist/{controller=Account}/{action=Profile}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();

              

                //endpoints.MapControllerRoute(
                //    name: "defaultTherapists",
                //    pattern: "{controller=Therapists}/{action=All}/{id?}");
                //endpoints.MapRazorPages();
            });

            CreateClientRole(serviceProvider).Wait();
            CreateTherapistRole(serviceProvider).Wait();
            CreateAdminRole(serviceProvider).Wait();
            CreateAdminUser(serviceProvider).Wait();
        }
    }
}
