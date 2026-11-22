using MadaFinishing.Web.Data;
using MadaFinishing.Web.Inquiries;
using MadaFinishing.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

const string setupManagerArgument = "--setup-manager";
const string setupCoordinatorArgument = "--setup-coordinator";
var setupRole = args.FirstOrDefault(argument => argument is setupManagerArgument or setupCoordinatorArgument) switch
{
    setupManagerArgument => StaffUser.ManagerRole,
    setupCoordinatorArgument => StaffUser.CoordinatorRole,
    _ => null
};
var builder = WebApplication.CreateBuilder(args
    .Where(argument => argument is not (setupManagerArgument or setupCoordinatorArgument)).ToArray());

builder.Services.AddDbContext<MadaFinishingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MadaFinishing")));
builder.Services.AddIdentity<StaffUser, IdentityRole>(options => options.User.RequireUniqueEmail = true)
    .AddEntityFrameworkStores<MadaFinishingDbContext>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/SignIn";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
builder.Services.AddScoped<InquiryService>();
builder.Services.AddControllersWithViews();

await using var app = builder.Build();

if (setupRole is not null)
{
    await using var scope = app.Services.CreateAsyncScope();
    await StaffSetup.RunAsync(scope.ServiceProvider, app.Configuration, setupRole);
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inquiries}/{action=Index}/{id?}")
    .WithStaticAssets();

await app.RunAsync();

public partial class Program
{
}
