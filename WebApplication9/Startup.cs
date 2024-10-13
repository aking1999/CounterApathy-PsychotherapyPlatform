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
using Framework.Notifications;
using Framework.RouteMiddlewares;
using Framework.Emails;
using Framework.Interfaces;
using Framework.Implementations;
using Framework.ActionFilters;
using Winton.AspNetCore.Seo;
using System.Collections.Generic;
using Winton.AspNetCore.Seo.Robots;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Rewrite;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication9
{
    public class Startup
    {
        public Startup(IWebHostEnvironment _environment)
        {
            Configuration = new ConfigurationBuilder()
                                .SetBasePath(_environment.ContentRootPath)
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                .AddJsonFile($"appsettings.{_environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                                .Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);

            var appName = Configuration.GetSection("Application:AppName")?.Value;

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            //error HTTP Error 500.30 - ANCM In-Process Start Failure
            //https://stackoverflow.com/questions/53811569/http-error-500-30-ancm-in-process-start-failure
            //services.AddIdentity<CustomClient, IdentityRole>()
            //    .AddErrorDescriber<SerbianIdentityErrorDescriber>();

            services.AddIdentity<CustomClient, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                // if LowercaseQueryStrings is set to true, it will mess up token passing
                // for ForgotPassword and EmailVerification and we will get Invalid Token error.
                options.LowercaseQueryStrings = false;
            });

            services.AddSeoWithDefaultRobots();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = $"{appName}.Authentication";
                options.LoginPath = new PathString("/autorizacija/logovanje");
                options.AccessDeniedPath = new PathString("/error/zabranjen-pristup");
                //options.LogoutPath = new PathString("/[your-path]");
            });

            services.Configure<CookieTempDataProviderOptions>(options => options.Cookie.Name = $"{appName}.Temporary");

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(typeof(UserActivityLoggerAttribute));
                options.Filters.Add(typeof(UserAccountInformationLoggerAttribute));
                //options.Filters.Add(typeof(EmailConfirmationCheckerAttribute));
            });

            services.AddRazorPages();

            services.AddSession(options =>
            {
                options.Cookie.Name = $"{appName}.Session";
            });

            services.AddHttpClient();
            services.AddHttpContextAccessor();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                .AddScoped(x =>
                    x.GetRequiredService<IUrlHelperFactory>()
                        .GetUrlHelper(x.GetRequiredService<IActionContextAccessor>().ActionContext));

            services.AddScoped<IPasswordHasher<CustomClient>, PasswordHasher<CustomClient>>();

            services.AddSingleton<UnbookUnpaidSessionService>();
            services.AddHostedService<UnbookUnpaidSessionService>();
            services.AddHostedService<TherapistSessionPaymentService>();
            services.AddHostedService<RateSessionReminderService>();
            services.AddHostedService<UpcomingSessionNotificationSenderService>();
            services.AddHostedService<OldNotificationDeleterService>();

            services.AddTransient<INotificationRepository, NotificationRepository>();
            services.AddSignalR();
            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.Configure<MailSettings>(Configuration.GetSection("MailSettings"));

            //ovde mozda bude trebalo da se stavi scope ili singleton
            services.AddTransient<IErrorLogger, ErrorLogger>();
            services.AddTransient<IMailService, MailService>();
            services.AddSingleton<IDateTimeHelper, DateTimeHelper>();

            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = $"{appName}.AntiForgery";
            });

            services.AddSeo(
                options =>
                {
                    options.Sitemap.Urls = Helpers.SeoHelper.SitemapUrls;
                    options.RobotsTxt.UserAgentRecords = new List<UserAgentRecord>
                    {
                        new UserAgentRecord
                        {
                            Disallow = Helpers.SeoHelper.DisabledUrls.Concat(Helpers.SeoHelper.DisabledContent),
                            NoIndex = Helpers.SeoHelper.DisabledUrls
                        }
                    };
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            Stripe.StripeConfiguration.ApiKey = Configuration.GetSection("StripeSettings").Get<Models.StripeSettings>().ApiKey;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/error/{0}");
                app.UseExceptionHandler("/error/neobradjena-greška");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //zbog google seo ne koristim ovo jer vraca status code 307, a 301 je bolji, AddRedirectToHttpsPermanent vraca 301
            //app.UseHttpsRedirection();
            var rewriteOptions = new RewriteOptions();
            rewriteOptions.AddRedirectToHttpsPermanent();
            app.UseRewriter(rewriteOptions);
            app.UseStaticFiles();

            // !!! ovo mi moze praviti problem ako budem menjao direktorijum ili se neka lokacija bude menjala.
            // !!! OVO TRENUTNO RADI !!!
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(
            //    Path.Combine(env.ContentRootPath, "../Framework/Emails/EmailTemplates")),
            //    RequestPath = "/Templates"
            //});

            //rewriteOptions.Add(new RedirectLowerCaseRouteRule(new SystemErrorLogger()));
            rewriteOptions.Add(new RedirectToNonWwwRouteRule(new SystemErrorLogger()));
            rewriteOptions.AddRedirect("(.*)/$", "$1", 301);
            app.UseRewriter(rewriteOptions);

            //Kad se unese /base u url, ne prikaze custom 404 error stranu,
            //nego default googlovu zbog toga sto je IgnoreRouteMiddleware pre UseStatusCodePagesWithReExecute
            app.UseMiddleware<IgnoreRouteMiddleware>();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<NotificationHub>($"/{NotificationHub.Url}");

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
            });

            ((IFileRepository)new FileRepository()).PrepareToRun(env);
            ((IContextSetup)new ContextSetup(serviceProvider)).PrepareToRun();
        }
    }
}
