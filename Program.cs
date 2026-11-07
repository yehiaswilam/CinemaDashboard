using CinemaDashboard.Data;
using CinemaDashboard.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services
    .AddDataAccess(builder.Configuration)
    .AddAppIdentity(builder.Configuration)
    .AddAppServices(builder.Configuration);

var app = builder.Build();

// Applies migrations + creates roles and the first Admin
await DbSeeder.SeedAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Customer/Home/Error");
    app.UseHsts();
}

// HTTPS disabled for local development (runs on http://localhost:5070)
// app.UseHttpsRedirection();

// Fixed culture so decimal prices (e.g. 120.50) bind correctly regardless of the Windows locale
app.UseRequestLocalization("en-US");

app.UseStaticFiles();   // serves wwwroot, including uploaded images
app.UseRouting();

app.UseAuthentication(); // who are you?  (must come before UseAuthorization)
app.UseAuthorization();  // are you allowed?

// Every URL starts with its area:  /Admin/Movies/Edit/5 · /Identity/Account/Login · /Customer/Movies/Details/3
// "/" alone opens the Customer area (the public site).
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
