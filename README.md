# نظام الحصر الجغرافي للأوقاف (WaqfGIS)
## Waqf GIS Platform - Phase 1

### 📋 نظرة عامة
نظام متكامل لإدارة وعرض المساجد والعقارات الوقفية والدوائر الوقفية في العراق على خريطة تفاعلية.

---

## 🚀 البدء السريع

### المتطلبات
- .NET 8 SDK
- SQL Server (LocalDB أو Express)
- Visual Studio 2022 أو VS Code

### خطوات التشغيل

1. **استنساخ المشروع**
```bash
cd D:\Projects\WaqfGIS
```

2. **تثبيت الحزم**
```bash
dotnet restore src/WaqfGIS.Web/WaqfGIS.Web.csproj
```

3. **إنشاء قاعدة البيانات**
```bash
cd src/WaqfGIS.Web
dotnet ef migrations add InitialCreate --project ../WaqfGIS.Infrastructure
dotnet ef database update --project ../WaqfGIS.Infrastructure
```

4. **تشغيل التطبيق**
```bash
dotnet run --project src/WaqfGIS.Web
```

5. **فتح المتصفح**
```
https://localhost:7000
```

---

## مزامنة تحديثات GIS من المشروع الاصلي

لاستقبال اي تحديث جديد من صاحب مشروع GIS عندك بدون خطوات يدوية كثيرة:

1. **اعداد upstream مرة واحدة فقط**
```powershell
.\scripts\setup-upstream.ps1 -UpstreamUrl "ضع_رابط_المستودع_الاصلي" -UpstreamBranch "main"
```

2. **مزامنة التحديثات لاحقا**
```powershell
.\scripts\sync-upstream.ps1
```

### ماذا يحدث اثناء المزامنة؟
- يتم سحب اخر تحديثات من upstream.
- يتم تنفيذ rebase على فرعك الحالي للحفاظ على تاريخ نظيف.
- لو عندك تعديلات محلية غير محفوظة، السكربت يعمل stash تلقائيا ثم يعيدها بعد انتهاء المزامنة.

### عند وجود تعارضات
- اصلح التعارضات في الملفات.
- ثم نفذ:
```powershell
git add .
git rebase --continue
```
- اذا احتجت الغاء المزامنة:
```powershell
git rebase --abort
```

---

## 🔐 بيانات تسجيل الدخول الافتراضية

| المستخدم | كلمة المرور |
|----------|-------------|
| admin    | Admin@123456 |

---

## 📁 هيكل المشروع

```
WaqfGIS/
├── src/
│   ├── WaqfGIS.Core/           # الكيانات والواجهات
│   │   ├── Entities/           # كيانات قاعدة البيانات
│   │   ├── Interfaces/         # واجهات المستودعات
│   │   └── Enums/              # التعدادات
│   │
│   ├── WaqfGIS.Infrastructure/ # طبقة البيانات
│   │   ├── Data/               # DbContext والتكوينات
│   │   └── Repositories/       # تنفيذ المستودعات
│   │
│   ├── WaqfGIS.Services/       # منطق الأعمال
│   │   ├── MosqueService.cs
│   │   ├── PropertyService.cs
│   │   ├── OfficeService.cs
│   │   └── ReportService.cs
│   │
│   └── WaqfGIS.Web/            # واجهة المستخدم MVC
│       ├── Controllers/
│       ├── Views/
│       ├── Models/             # ViewModels
│       └── wwwroot/            # الملفات الثابتة
│
└── README.md
```

---

## 📊 الجداول الرئيسية

| الجدول | الوصف |
|--------|-------|
| Provinces | المحافظات |
| Districts | الأقضية |
| SubDistricts | النواحي |
| WaqfOffices | الدوائر الوقفية |
| Mosques | المساجد والجوامع |
| WaqfProperties | العقارات الوقفية |
| MosqueTypes | أنواع المساجد |
| PropertyTypes | أنواع العقارات |

---

## 🗺️ الميزات

- ✅ خريطة تفاعلية مع Leaflet
- ✅ إدارة المساجد (CRUD)
- ✅ إدارة العقارات (CRUD)
- ✅ إدارة الدوائر الوقفية
- ✅ لوحة تحكم مع إحصائيات
- ✅ نظام مصادقة آمن
- ✅ دعم اللغة العربية (RTL)
- ✅ تصميم متجاوب

---

## 🔧 التقنيات المستخدمة

- **Backend:** ASP.NET Core 8 MVC
- **Database:** SQL Server + Entity Framework Core 8
- **GIS:** NetTopologySuite
- **Frontend:** Bootstrap 5 RTL + Leaflet.js
- **Charts:** Chart.js
- **Icons:** Font Awesome 6

---

## 📞 الدعم الفني

للاستفسارات والدعم الفني، يرجى التواصل مع فريق التطوير.

---

© 2024 ديوان الوقف - جميع الحقوق محفوظة
