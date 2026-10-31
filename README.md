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

| | |
|---|---|
| `ApplicationUser` | بيورث من `IdentityUser` + `FullName`, `CreatedAt` |
| Roles | `Admin` (يدخل الداشبورد) و `User` (أي حد يعمل Register — بيتصفح الموقع بس) |
| صفحات | Login · Register · Logout · Access Denied |
| Users page | الأدمن يقدر يرقّي يوزر لأدمن، يقفل/يفتح حساب، أو يمسحه |
| الحماية | كل Controllers الـ Admin Area بتورث من `AdminController` اللي عليه `[Authorize(Roles = "Admin")]` |

## SOLID — فين كل مبدأ في الكود

| المبدأ | التطبيق |
|---|---|
| **S** — Single Responsibility | الـ Controllers بقت رفيعة (HTTP بس)، والـ business rules في `Services/Implementations`، والـ data access في `Repositories`. التحقق من الصور (`ImageValidator`) منفصل عن حفظها (`LocalFileStorage`). الـ DI كله في `Extensions/ServiceCollectionExtensions.cs`. |
| **O** — Open/Closed | `Repository<T>` واحد شغال لأي Entity جديدة من غير كود جديد. قواعد رفع الصور في `appsettings.json` (`ImageUpload`) تتغير من غير ما تلمس الكود. |
| **L** — Liskov Substitution | أي implementation لـ `IFileStorage` (مثلاً Azure Blob بدل الهارد) تتحط مكان `LocalFileStorage` والسيستم يفضل شغال بنفس الشكل. |
| **I** — Interface Segregation | Interfaces صغيرة ومتخصصة: `IImageValidator` ≠ `IFileStorage`، `IAccountService` (دخول/تسجيل) ≠ `IUserService` (إدارة اليوزرز)، `ILookupService` للـ dropdowns بس. |
| **D** — Dependency Inversion | كل Controller و Service بياخد Interfaces في الـ constructor (`ICategoryService`, `IUnitOfWork`...) ومفيش حد بيعمل `new` لـ class أو بيلمس `DbContext` غير الـ Repository. |

## هيكل المشروع

```
Areas/
  Admin/          Controllers (AdminController base, Home, Movies, Categories, Cinemas, Actors, Users) + Views
  Customer/       Controllers (Home, Movies, Actors) + Views — public, read-only
  Identity/       AccountController + Views (Login, Register, AccessDenied)
Constants/        AppRoles
Data/             ApplicationDbContext (IdentityDbContext) + DbSeeder
Extensions/       DI registration + ModelState helper
Models/           Entities + ApplicationUser
Repositories/     IRepository<T>, Repository<T>, IUnitOfWork, UnitOfWork
Services/
  Common/         ServiceResult
  Files/          IImageValidator, IFileStorage, ImageUploadOptions
  Interfaces/     I...Service
  Implementations/
ViewModels/
```
