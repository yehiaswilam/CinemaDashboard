using CinemaDashboard.Data;
using CinemaDashboard.Models;
using CinemaDashboard.Repositories;
using CinemaDashboard.Services.Files;
using CinemaDashboard.Services.Implementations;
using CinemaDashboard.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CinemaDashboard.Extensions;

/// <summary>
/// All dependency-injection wiring lives here, so Program.cs stays short.
/// This is the one place that decides which implementation is used for each interface (DIP).
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddAppIdentity(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Identity/Account/Login";
            options.LogoutPath = "/Identity/Account/Logout";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
        });

        return services;
    }

    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
    {
        // Files
        services.Configure<ImageUploadOptions>(config.GetSection(ImageUploadOptions.SectionName));
        services.AddSingleton<IImageValidator, ImageValidator>();
        services.AddScoped<IFileStorage, LocalFileStorage>();

        // Business services
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICinemaService, CinemaService>();
        services.AddScoped<IActorService, ActorService>();
        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<IDashboardService, DashboardService>();

        // Identity-related
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
