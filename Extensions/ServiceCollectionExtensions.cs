using CinemaDashboard.Data;
using CinemaDashboard.Models;
using CinemaDashboard.Repositories;
using CinemaDashboard.Services.Email;
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

    public static IServiceCollection AddAppIdentity(this IServiceCollection services, IConfiguration config)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

                // Users must click the link in the confirmation email before they can sign in
                options.SignIn.RequireConfirmedEmail = true;
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

        // Confirmation / reset links stay valid for 24 hours
        services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromHours(24));

        // Google sign-in is switched on only when its keys are in appsettings / user-secrets
        var googleId = config["Authentication:Google:ClientId"];
        var googleSecret = config["Authentication:Google:ClientSecret"];
        if (!string.IsNullOrWhiteSpace(googleId) && !string.IsNullOrWhiteSpace(googleSecret))
        {
            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = googleId!;
                options.ClientSecret = googleSecret!;
            });
        }

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

        // Email: real SMTP when EmailSettings is filled in, otherwise print to the console (same interface — LSP)
        var emailSection = config.GetSection(EmailSettings.SectionName);
        services.Configure<EmailSettings>(emailSection);
        if (emailSection.Get<EmailSettings>()?.IsConfigured == true)
            services.AddScoped<IEmailService, SmtpEmailService>();
        else
            services.AddScoped<IEmailService, ConsoleEmailService>();

        services.AddHttpContextAccessor();
        services.AddScoped<IAccountLinkBuilder, AccountLinkBuilder>();

        // Identity-related (small, focused interfaces — ISP)
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IEmailConfirmationService, EmailConfirmationService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
