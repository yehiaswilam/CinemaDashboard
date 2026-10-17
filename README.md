# Cinema Dashboard — ASP.NET Core MVC (.NET 10)

لوحة تحكم لإدارة سينما: Categories, Cinemas, Movies, Actors + صفحة رئيسية فيها إحصائيات.

## التشغيل (Visual Studio 2026 / .NET 10)

1. افتح `CinemaDashboard.csproj` في Visual Studio.
2. افتح `appsettings.json` وظبط اسم السيرفر في الـ Connection String
   على حسب اللي بتكتبه في SSMS في خانة **Server name**:
   - `Server=.` → لو السيرفر الافتراضي (MSSQLSERVER)
   - `Server=.\\SQLEXPRESS` → لو SQL Express (لاحظ الـ `\\` في JSON)
   - `Server=(localdb)\\MSSQLLocalDB` → لو LocalDB
3. من **Tools → NuGet Package Manager → Package Manager Console**:
   ```
   Add-Migration InitialCreate
   Update-Database
   ```
   (أو من الـ terminal: `dotnet ef migrations add InitialCreate` ثم `dotnet ef database update`)
4. اعمل Refresh في SSMS هتلاقي داتابيز `CinemaDashboardDb` فيها الجداول.
5. شغّل المشروع (F5).

> ترتيب الإدخال: ضيف **Cinema** و **Actors** الأول (الـ Categories متضافة جاهزة)، وبعدين ضيف الـ **Movies**.

## الجداول

| Table | الأعمدة المهمة |
|---|---|
| Categories | Id, Name, Description |
| Cinemas | Id, Name, Address, Description, Img |
| Actors | Id, Name, Bio, Img |
| Movies | Id, Name, Description, Price, Status, DateTime, MainImg, CategoryId, CinemaId |
| MovieImages | Id, Img, MovieId  ← الـ SubImages |
| MovieActors | MovieId, ActorId  ← جدول وسيط Many-to-Many (List&lt;Actor&gt;) |

## هيكل المشروع

```
Controllers/   Home (Dashboard), Categories, Cinemas, Actors, Movies
Models/        Entities + MovieStatus enum
Data/          ApplicationDbContext (Fluent API + seed categories)
Services/      FileService (رفع/مسح الصور في wwwroot/images)
ViewModels/    MovieFormVM, DashboardVM
Views/         Razor views (Bootstrap 5)
```

## ملاحظات

- الصور بتتحفظ في `wwwroot/images/{cinemas|actors|movies|movies/sub}` والـ DB بيتخزن فيها المسار بس.
- مسموح: jpg / jpeg / png / gif / webp — حد أقصى 5MB للصورة.
- مينفعش تمسح Category أو Cinema فيها أفلام (DeleteBehavior.Restrict) — بيظهر رسالة.
- مسح فيلم بيمسح صوره والـ SubImages وعلاقاته بالممثلين.
- الـ Status: Upcoming / Now Showing (Available) / Expired — بيتخزن كـ string في الداتابيز.
