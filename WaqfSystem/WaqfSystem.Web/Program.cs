using System;
using System.Text;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using WaqfSystem.Application;
using WaqfSystem.Application.Services;
using WaqfSystem.Application.Validators;
using WaqfSystem.Infrastructure;
using WaqfSystem.Infrastructure.BackgroundJobs;
using WaqfSystem.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. Layered DI Registration
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddAdminPanel(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<CreateRoleValidator>();
builder.Services.AddScoped<IMissionService, MissionService>();
builder.Services.AddSignalR();
builder.Services.AddHangfire(config => config.UseMemoryStorage());
builder.Services.AddHangfireServer();

// 2. Dual Authentication (Cookie for MVC, JWT for API)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "WaqfSystem",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MobileApp",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "VERY_SECRET_KEY_FOR_WAQF_SYSTEM_2024_MUST_BE_LONG_ENOUGH"))
    };
});

// 3. MVC + API Controllers
builder.Services.AddControllersWithViews(options =>
{
    options.MaxIAsyncEnumerableBufferLimit = 0;
});
builder.Services.AddRazorPages();

// Session support for the property creation wizard
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".WaqfWizard.Session";
});

// 4. Swagger with JWT Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Waqf Property Census API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

// 5. Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Waqf API v1"));
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseAdminPanel();
app.UseSession(); // must be before MapControllerRoute
app.UseHangfireDashboard("/jobs");

app.MapHub<MissionHub>("/hubs/missions");

// 6. Routing (MVC + Mobile Area)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

RecurringJob.AddOrUpdate<SmartSuggestionRefresher>(
    "smart-suggestions-refresh",
    x => x.ExecuteAsync(),
    "0 7 * * *");

RecurringJob.AddOrUpdate<ScheduleOverdueChecker>(
    "schedule-overdue-checker",
    x => x.ExecuteAsync(),
    "30 6 * * *");

RecurringJob.AddOrUpdate<ContractExpiryNotifier>(
    "contract-expiry-notifier",
    x => x.ExecuteAsync(),
    "0 8 * * *");

app.Run();
