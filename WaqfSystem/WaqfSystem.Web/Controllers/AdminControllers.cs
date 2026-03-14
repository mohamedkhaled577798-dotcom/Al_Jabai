using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfSystem.Core.Interfaces;
using WaqfSystem.Core.Entities;

namespace WaqfSystem.Web.Controllers
{
    [Authorize(Roles = "SYS_ADMIN")]
    public class UserController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _unitOfWork.GetQueryable<WaqfSystem.Core.Entities.User>().Include(u => u.Role).Include(u => u.Governorate).ToListAsync();
            return View(users);
        }
    }

    [Authorize]
    public class ReportController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult PropertyExport()
        {
            // Placeholder for Excel/PDF export logic
            return PhysicalFile("path/to/report.pdf", "application/pdf");
        }
    }
}
