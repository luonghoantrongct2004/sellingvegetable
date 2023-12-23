using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Notyf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FruityFresh.Extension;
using FruityFresh.Helper;
using FruityFresh.Models;

namespace FruityFresh.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminAccountsController : Controller
    {
        private readonly FruityFreshContext _context;
        public INotyfService _notyfService { get; }
		private readonly ILogger<FruityFreshContext> _logger;
		private readonly IWebHostEnvironment _hostingEnvironment;
		public AdminAccountsController(FruityFreshContext context, IWebHostEnvironment hostingEnvironment, ILogger<FruityFreshContext> logger, INotyfService notyfService)
		{
			_context = context;
			_hostingEnvironment = hostingEnvironment;
			_logger = logger;
			_notyfService = notyfService;
		}

        // GET: Admin/AdminAccounts
        public async Task<IActionResult> Index()
        {
            return View(await _context.AdminAccounts.ToListAsync());
        }

        // GET: Admin/AdminAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adminAccount = await _context.AdminAccounts
                .FirstOrDefaultAsync(m => m.AdminId == id);
            if (adminAccount == null)
            {
                return NotFound();
            }

            return View(adminAccount);
        }

        // GET: Admin/AdminAccounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminAccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AdminId,Username,Password,FullName,Email,Phone,LastLogin,CreatedAt,DeletedAt")] 
        AdminAccount adminAccount,IFormFile fileThumb)
        {
            if (ModelState.IsValid)
            {
                try
                {
					if (fileThumb != null && fileThumb.Length > 0)
					{
						try
						{
							adminAccount.FullName = Utilities.ToTitleCase(adminAccount.FullName);
							string extension = Path.GetExtension(fileThumb.FileName);
							string Avatar = Utilities.SEOUrl(adminAccount.FullName) + extension;

							adminAccount.Avatar = await Utilities.UploadFile(fileThumb, "AvatarAdmin", Avatar.ToLower(), _hostingEnvironment);
						}
						catch (Exception ex)
						{
							_logger.LogError($"Exception in: {ex.Message}");
						}
					}
					else
					{
						ModelState.AddModelError("fileThumb", "Please select a file to upload.");
						_logger.LogInformation("Avatar is null or empty in file");
						adminAccount.Avatar = "\\Avatars\\AvatarAdmin\\default.jpg";
					}
                   /* string pass = adminAccount.Password.ToMD5();

                    adminAccount.Password = pass;*/
                    _context.Add(adminAccount);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Create admin account " + adminAccount.Username + " successful.");
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(AdminAccountExists(adminAccount.AdminId))
                    {
                        _notyfService.Success("Was error occur.");
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(adminAccount);
        }

        // GET: Admin/AdminAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adminAccount = await _context.AdminAccounts.FindAsync(id);

            if (adminAccount == null)
            {
                return NotFound();
            }
            return View(adminAccount);
        }

        // POST: Admin/AdminAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AdminId,Username,Password,FullName,Email,Phone,LastLogin,CreatedAt,DeletedAt")] 
        AdminAccount adminAccount, IFormFile fileThumb)
        {
            if (id != adminAccount.AdminId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
					var existingAdmin = await _context.AdminAccounts.FindAsync(id);
					if (existingAdmin == null)
					{
						return NotFound();
					}
					_context.Entry(existingAdmin).State = EntityState.Detached;
					existingAdmin.FullName = adminAccount.FullName;
					existingAdmin.Email = adminAccount.Email;
					existingAdmin.Phone = adminAccount.Phone;
					if (fileThumb != null && fileThumb.Length > 0)
					{
						try
						{
							string oldAvatarPath = existingAdmin.Avatar;

							if (!string.IsNullOrEmpty(oldAvatarPath))
							{
								if (System.IO.File.Exists(oldAvatarPath))
								{
									System.IO.File.Delete(oldAvatarPath);
								}
							}
							string extension = Path.GetExtension(fileThumb.FileName);
							string Avatar = Utilities.SEOUrl(adminAccount.FullName) + extension;
							existingAdmin.Avatar = await Utilities.UploadFile(fileThumb, "AvatarAdmin", Avatar.ToLower(), _hostingEnvironment);
						}
						catch (Exception ex)
						{
							_logger.LogError($"Exception in: {ex.Message}");
						}
					}
					else
					{
						ModelState.AddModelError("fileThumb", "Please select a file to upload.");
						_logger.LogInformation("Avatar is null or empty in file");
						existingAdmin.Avatar = adminAccount.Avatar;
					}
					_context.Update(adminAccount);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Update admin account " + adminAccount.Username + " successful.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdminAccountExists(adminAccount.AdminId))
                    {
                        _notyfService.Success("Was error occur.");
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(adminAccount);
        }

        // GET: Admin/AdminAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adminAccount = await _context.AdminAccounts
                .FirstOrDefaultAsync(m => m.AdminId == id);
            if (adminAccount == null)
            {
                return NotFound();
            }

            return View(adminAccount);
        }

        // POST: Admin/AdminAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var adminAccount = await _context.AdminAccounts.FindAsync(id);
            _context.AdminAccounts.Remove(adminAccount);
            await _context.SaveChangesAsync();
            _notyfService.Success("Delete admin account " + adminAccount.Username + " successful.");
            return RedirectToAction(nameof(Index));
        }

        private bool AdminAccountExists(int id)
        {
            return _context.AdminAccounts.Any(e => e.AdminId == id);
        }
    }
}
