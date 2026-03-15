using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace WaqfSystem.Infrastructure.Authorization
{
    public class DynamicAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public DynamicAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode != StatusCodes.Status403Forbidden)
            {
                return;
            }

            var factory = context.RequestServices.GetService<ITempDataDictionaryFactory>();
            var tempData = factory?.GetTempData(context);
            var permission = tempData != null && tempData.TryGetValue("PermissionDenied", out var value)
                ? value?.ToString()
                : null;

            if (string.IsNullOrWhiteSpace(permission))
            {
                permission = context.Request.Query["permission"].ToString();
            }

            var isAjax = string.Equals(context.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
                || context.Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);

            if (isAjax)
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync($"{{\"success\":false,\"error\":\"PERMISSION_DENIED\",\"message\":\"ليس لديك صلاحية للوصول إلى هذه الصفحة\",\"permissionRequired\":\"{permission}\"}}");
                return;
            }

            if (!context.Response.HasStarted)
            {
                context.Response.Redirect($"/Admin/AccessDenied?permission={Uri.EscapeDataString(permission ?? string.Empty)}");
            }
        }
    }
}
