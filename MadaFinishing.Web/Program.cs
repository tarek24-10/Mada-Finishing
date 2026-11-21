using MadaFinishing.Web.Data;
using MadaFinishing.Web.Inquiries;
using MadaFinishing.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

const string setupManagerArgument = "--setup-manager";
var setupManager = args.Contains(setupManagerArgument);
var builder = WebApplication.CreateBuilder(args.Where(argument => argument != setupManagerArgument).ToArray());

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

if (setupManager)
{
    await using var scope = app.Services.CreateAsyncScope();
    await ManagerSetup.RunAsync(scope.ServiceProvider, app.Configuration);
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
