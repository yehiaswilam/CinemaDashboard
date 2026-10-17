using CinemaDashboard.Data;
using CinemaDashboard.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IFileService, FileService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// HTTPS disabled for local development (runs on http://localhost:5070)
// app.UseHttpsRedirection();

// Fixed culture so decimal prices (e.g. 120.50) bind correctly regardless of the Windows locale
app.UseRequestLocalization("en-US");

app.UseStaticFiles();   // serves wwwroot, including uploaded images
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
