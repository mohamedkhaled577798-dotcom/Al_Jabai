using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Infrastructure.Data;
using WaqfGIS.Infrastructure.Repositories;
using WaqfGIS.Services;
using WaqfGIS.Services.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.UseNetTopologySuite()));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ═══════════════════════════════════════════════════
// تخزين الملفات الآمن المشفّر
// لتغيير مسار التخزين: عدّل "FileStorage:RootPath" في appsettings.json
// ═══════════════════════════════════════════════════
builder.Services.Configure<FileStorageSettings>(
    builder.Configuration.GetSection("FileStorage"));
builder.Services.AddSingleton<FileStorageSettings>(sp =>
    sp.GetRequiredService<IOptions<FileStorageSettings>>().Value);
builder.Services.AddSingleton<SecureFileStorageService>();

// Add Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<MosqueService>();
builder.Services.AddScoped<PropertyService>();
builder.Services.AddScoped<OfficeService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<ExcelExportService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<ImageUploadService>();

// GIS Services - Phase 2
builder.Services.AddScoped<WaqfGIS.Services.GIS.GeometryService>();
builder.Services.AddScoped<WaqfGIS.Services.GIS.SpatialAnalysisService>();
builder.Services.AddScoped<WaqfGIS.Services.GIS.LayerService>();
builder.Services.AddScoped<WaqfGIS.Services.WaqfLandService>();
builder.Services.AddScoped<WaqfGIS.Services.RoadService>();

// Advanced Services - Phase 3
builder.Services.AddScoped<ContractService>();
builder.Services.AddScoped<AlertService>();
builder.Services.AddScoped<ExcelImportService>();
builder.Services.AddScoped<PdfReportService>();
builder.Services.AddScoped<MaintenanceService>();
builder.Services.AddHostedService<WaqfGIS.Web.BackgroundServices.AlertBackgroundService>();
builder.Services.AddScoped<ServiceAssessmentService>();

// Add MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await DbSeeder.SeedAsync(context, userManager, roleManager);
}

// Configure pipeline
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
