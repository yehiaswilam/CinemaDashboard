# Cinema Dashboard — ASP.NET Core MVC (.NET 10)

موقع سينما متقسم لـ **3 Areas**، بتسجيل دخول بـ **ASP.NET Core Identity** وكود متقسم على مبادئ **SOLID**.

## Areas

| Area | الرابط | مين يدخلها | فيها إيه |
|---|---|---|---|
| **Customer** | `/` | أي حد | الأفلام المعروضة والجاية، بحث وفلترة بالـ Category، تفاصيل الفيلم والممثل — **عرض بس** |
| **Admin** | `/Admin` | Admin بس | الداشبورد + CRUD للأفلام والـ Categories والسينمات والممثلين + إدارة اليوزرز |
| **Identity** | `/Identity/Account/Login` | أي حد | Login · Register · Logout · Access Denied |

- كل Controller عليه `[Area("...")]`، وكل Area ليها `_ViewImports` و `_ViewStart` و `_Layout` خاصين بيها.
- كل Controllers الأدمن بتورث من `AdminController` اللي عليه `[Authorize(Roles = "Admin")]`،
  فمستحيل يوزر عادي يوصل لـ Add / Edit / Delete.
- الـ Route: `{area=Customer}/{controller=Home}/{action=Index}/{id?}`

## التشغيل

1. ظبط اسم السيرفر في `appsettings.json` → `ConnectionStrings:DefaultConnection`
   (نفس اللي في خانة Server name في SSMS، مثلاً `Server=.\\SQLEXPRESS`).
2. من Developer PowerShell في فولدر المشروع:
   ```
   dotnet ef migrations add InitialCreate
   ```
3. شغّل المشروع (F5). عند التشغيل المشروع **بيعمل Update-Database لوحده**، وبيعمل الـ Roles وأول Admin.
4. افتح `http://localhost:5070` (الموقع) أو `http://localhost:5070/Admin` (الداشبورد) وادخل بـ:
   - Email: `admin@cinema.com`
   - Password: `Admin@123`
   (تقدر تغيرهم من `AdminUser` في `appsettings.json` قبل أول تشغيل)

## Identity

| Feature | التفاصيل |
|---|---|
| Register | بيبعت إيميل تأكيد — مينفعش يعمل Login قبل ما يأكد (`RequireConfirmedEmail`) |
| Confirm / Resend | لينك التأكيد صالح 24 ساعة، وفي صفحة لإعادة إرساله |
| Login / Logout | + Remember me + قفل الحساب 5 دقايق بعد 5 محاولات غلط |
| Forgot / Reset password | لينك على الإيميل → باسورد جديد |
| Login with Google | بيربط الحساب لو الإيميل موجود، أو بيعمل حساب جديد |
| Profile | تعديل الاسم والموبايل وصورة البروفايل |
| Change / Set password | تغيير الباسورد، أو تعيين باسورد لحساب اتعمل بـ Google |
| Roles | `Admin` (الداشبورد) و `User` (الموقع) |
| Users (Admin) | ترقية لأدمن، قفل/فتح، مسح، وتأكيد إيميل يدوي |

### إعداد الإيميل (Gmail) و Google — من غير ما الأسرار تترفع على GitHub

الـ repo ده public، فالباسوردات والمفاتيح **مش بتتكتب في `appsettings.json`**، بتتحط في **User Secrets** (بتتخزن على جهازك بس):

```
dotnet user-secrets set "EmailSettings:UserName" "you@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "xxxx xxxx xxxx xxxx"     # Gmail App Password
dotnet user-secrets set "Authentication:Google:ClientId" "....apps.googleusercontent.com"
dotnet user-secrets set "Authentication:Google:ClientSecret" "...."
```

- لو الإيميل مش متظبط، الإيميلات **بتتطبع في الـ Console** (بلينكاتها) بدل ما تتبعت — تقدر تجرّب بيها.
- لو Google مش متظبط، زرار "Continue with Google" مش بيظهر.
- Google redirect URI: `http://localhost:5070/signin-google`

## SOLID — فين كل مبدأ في الكود

| المبدأ | التطبيق |
|---|---|
| **S** — Single Responsibility | الـ Controllers بقت رفيعة (HTTP بس)، والـ business rules في `Services/Implementations`، والـ data access في `Repositories`. التحقق من الصور (`ImageValidator`) منفصل عن حفظها (`LocalFileStorage`). الـ DI كله في `Extensions/ServiceCollectionExtensions.cs`. |
| **O** — Open/Closed | `Repository<T>` واحد شغال لأي Entity جديدة من غير كود جديد. قواعد رفع الصور في `appsettings.json` (`ImageUpload`) تتغير من غير ما تلمس الكود. |
| **L** — Liskov Substitution | `IEmailService` ليها implementationين: `SmtpEmailService` (بيبعت فعلاً) و `ConsoleEmailService` (بيطبع) — أي واحدة فيهم تتحط مكان التانية والسيرفسز مش بتحس. ونفس الفكرة مع `IFileStorage`. |
| **I** — Interface Segregation | Interfaces صغيرة ومتخصصة: `IImageValidator` ≠ `IFileStorage`، والـ Identity متقسم لـ `IAccountService` (دخول/تسجيل) و `IEmailConfirmationService` و `IPasswordService` و `IProfileService` و `IUserService` (إدارة اليوزرز)، و `ILookupService` للـ dropdowns بس. |
| **D** — Dependency Inversion | كل Controller و Service بياخد Interfaces في الـ constructor (`ICategoryService`, `IUnitOfWork`...) ومفيش حد بيعمل `new` لـ class أو بيلمس `DbContext` غير الـ Repository. |

## هيكل المشروع

```
Areas/
  Admin/          Controllers (AdminController base, Home, Movies, Categories, Cinemas, Actors, Users) + Views
  Customer/       Controllers (Home, Movies, Actors) + Views — public, read-only
  Identity/       AccountController (login, register, confirm, forgot/reset, Google) + ProfileController
Constants/        AppRoles
Data/             ApplicationDbContext (IdentityDbContext) + DbSeeder
Extensions/       DI registration + ModelState helper
Models/           Entities + ApplicationUser
Repositories/     IRepository<T>, Repository<T>, IUnitOfWork, UnitOfWork
Services/
  Common/         ServiceResult
  Email/          IEmailService (SMTP + Console), EmailTemplates, AccountLinkBuilder
  Files/          IImageValidator, IFileStorage, ImageUploadOptions
  Interfaces/     I...Service
  Implementations/
ViewModels/
```
