using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Bulky.Utility;
using Bulky.Models;
using Microsoft.Extensions.DependencyInjection;
using Stripe;
using Bulky.DataAccess.DbInitializer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


//injecting dbContext - connectionString for project
builder.Services.AddDbContext<ApplicationDBContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("ApplicationConnectionStrings")));

//fetching stripe settings for payment getway from appsettings
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

//identity auto injected service to access
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDBContext>().AddDefaultTokenProviders();

//adding session to the servicess now add session in request pipeline below in app.UseSession();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(option => 
{
    option.IdleTimeout = TimeSpan.FromMinutes(100);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

//initialise dbinitializer when project run 1st time 
//it will create user / role and migration if any requiere / pending
builder.Services.AddScoped<IDbInitializer, DbInitializer>();


//to access razor page in identity injection -- in pipeline app.MapRazorPages();
builder.Services.AddRazorPages();

//injecting Repositoruy - add scoped one for one request
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//injecting emailsender after role managemt it trow error for email to solve this 
//implement emailsender 
builder.Services.AddScoped<IEmailSender, SendMail>();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

//it should be after identity injection
//accessdenie - login - logout not mapping due to which override mapping
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

//facebook injection - sign in with facebook
builder.Services.AddAuthentication().AddFacebook(option =>
{
    option.AppId = "yourkey";
    option.AppSecret = "yourkey";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
//after stripe package install from NuGet configer stripe 
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
SeedDatabase();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();

// when the project run 1st time it will create admin user / roles / migration all in one
void SeedDatabase()
{
    using(var scope= app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}