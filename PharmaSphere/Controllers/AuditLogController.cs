using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaSphere.Application.Interfaces;
using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Domain.Entities;

namespace PharmaSphere.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditLogController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _unitOfWork.AuditLogs.GetAllAsync();

            var result = logs
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            var log = await _unitOfWork.AuditLogs.GetByIdAsync(
                id,
                query => query.Include(x => x.User)
            );

            if (log == null)
                return NotFound();

            return View(log);
        }
    }
}