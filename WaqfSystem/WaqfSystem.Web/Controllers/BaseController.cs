using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace WaqfSystem.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        protected string? CurrentUserRole => User.FindFirst(ClaimTypes.Role)?.Value;
        protected int? CurrentUserGovernorateId => int.TryParse(User.FindFirst("GovernorateId")?.Value, out var id) ? id : null;

        protected void SuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        protected void ErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        protected void WarningMessage(string message)
        {
            TempData["WarningMessage"] = message;
        }
    }
}
