using AlJabai.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Infrastructure.Data;
using WaqfGIS.Infrastructure.Repositories;
using WaqfGIS.Services;
using WaqfGIS.Services.GIS;

var builder = WebApplication.CreateBuilder(args);

// 1. Add WaqfGIS DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("WaqfGISConnection"),
        x =>
        {
            x.UseNetTopologySuite();
            x.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        }));

// 2. Add AlJabai DbContext (Independent Database)
builder.Services.AddDbContext<AlJabaiDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("AlJabaiConnection"),
        x => x.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

// Add Identity using the GIS database (common users)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Register Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register GIS Services (The core functionality shared with AlJabai)
builder.Services.AddScoped<GeometryService>();
builder.Services.AddScoped<SpatialAnalysisService>();
builder.Services.AddScoped<LayerService>();

// Add common services
builder.Services.AddScoped<PropertyService>();
builder.Services.AddScoped<MosqueService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var gisDb = services.GetRequiredService<ApplicationDbContext>();
    var alJabaiDb = services.GetRequiredService<AlJabaiDbContext>();

    if (gisDb.Database.GetMigrations().Any())
    {
        gisDb.Database.Migrate();
    }
    else
    {
        gisDb.Database.EnsureCreated();
    }

    if (alJabaiDb.Database.GetMigrations().Any())
    {
        alJabaiDb.Database.Migrate();
    }
    else
    {
        alJabaiDb.Database.EnsureCreated();
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
