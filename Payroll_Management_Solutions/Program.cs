using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Payroll_Management_Solutions.Data;
using Payroll_Management_Solutions.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// DbContext
builder.Services.AddDbContext<PayrollDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddDbContext<PayrollDbContext>(options =>
//    options.UseSqlServer(
//        @"Server=(localdb)\MSSQLLocalDB;Database=PayrollDb;Trusted_Connection=True;TrustServerCertificate=True"));

//cookies
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<PayrollDbContext>()
    .AddDefaultTokenProviders();
//for payroll
builder.Services.AddScoped<PayrollService>();
//for notifications
builder.Services.AddScoped<NotificationRepository>();

//for cachee
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
//for visitor count
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
var app = builder.Build();

// Seed roles and default users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.SeedRolesAndUsers(services);
    }
    catch (Exception ex)
    {
        // Log errors if needed
        Console.WriteLine($"Error seeding users/roles: {ex.Message}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
//cookies
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

// MVC default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// Razor Pages (required for Identity)
app.MapRazorPages();

app.Run();

//var builder = WebApplication.CreateBuilder(args);

//// MVC + Razor Pages
//builder.Services.AddControllersWithViews();
//builder.Services.AddRazorPages();

//// DbContext
//builder.Services.AddDbContext<PayrollDbContext>(options =>
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("DefaultConnection")));
////builder.Services.AddDbContext<PayrollDbContext>(options =>
////    options.UseSqlServer(
////        @"Server=(localdb)\MSSQLLocalDB;Database=PayrollDb;Trusted_Connection=True;TrustServerCertificate=True"));

////cookies
//builder.Services.Configure<CookiePolicyOptions>(options =>
//{
//    options.CheckConsentNeeded = context => true;
//    options.MinimumSameSitePolicy = SameSiteMode.None;
//});

//// Identity
//builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
//{
//    options.Password.RequireDigit = true;
//    options.Password.RequiredLength = 6;
//})
//    .AddEntityFrameworkStores<PayrollDbContext>()
//    .AddDefaultTokenProviders();
////for payroll
//builder.Services.AddScoped<PayrollService>();
////for notifications
//builder.Services.AddScoped<NotificationRepository>();

//var app = builder.Build();

//// Seed roles and default users
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        await DbInitializer.SeedRolesAndUsers(services);
//    }
//    catch (Exception ex)
//    {
//        // Log errors if needed
//        Console.WriteLine($"Error seeding users/roles: {ex.Message}");
//    }
//}

//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();
////cookies
//app.UseCookiePolicy();
//app.UseAuthentication();
//app.UseAuthorization();

//// MVC default route
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");


//// Razor Pages (required for Identity)
//app.MapRazorPages();

//app.Run();
