# Cinema Dashboard — ASP.NET Core MVC (.NET 10)

لوحة تحكم لإدارة سينما: Categories, Cinemas, Movies, Actors + صفحة رئيسية فيها إحصائيات،
مع تسجيل دخول بـ **ASP.NET Core Identity** وكود متقسم على مبادئ **SOLID**.

## التشغيل

1. ظبط اسم السيرفر في `appsettings.json` → `ConnectionStrings:DefaultConnection`
   (نفس اللي في خانة Server name في SSMS، مثلاً `Server=.\\SQLEXPRESS`).
2. من Developer PowerShell في فولدر المشروع:
   ```
   dotnet ef migrations add InitialCreate
   ```
3. شغّل المشروع (F5). عند التشغيل المشروع **بيعمل Update-Database لوحده**، وبيعمل الـ Roles وأول Admin.
4. افتح `http://localhost:5070` وادخل بـ:
   - Email: `admin@cinema.com`
   - Password: `Admin@123`
   (تقدر تغيرهم من `AdminUser` في `appsettings.json` قبل أول تشغيل)

## Identity

| | |
|---|---|
| `ApplicationUser` | بيورث من `IdentityUser` + `FullName`, `CreatedAt` |
| Roles | `Admin` (يدخل الداشبورد) و `User` (أي حد يعمل Register) |
| صفحات | Login · Register · Logout · Access Denied |
| Users page | الأدمن يقدر يرقّي يوزر لأدمن، يقفل/يفتح حساب، أو يمسحه |
| الحماية | كل Controllers الداشبورد بتورث من `AdminController` اللي عليه `[Authorize(Roles = "Admin")]` |

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
Constants/        AppRoles
Controllers/      Base/AdminController + Home, Movies, Categories, Cinemas, Actors, Users, Account
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
Views/            + Account/, Users/, Shared/_AuthLayout
```
