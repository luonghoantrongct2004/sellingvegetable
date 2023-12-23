using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using PagedList.Core;
using FruityFresh.Helper;
using FruityFresh.Models;

namespace FruityFresh.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminCustomersController : Controller
    {
        private readonly FruityFreshContext _context;
        private readonly ILogger<FruityFreshContext> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public AdminCustomersController(FruityFreshContext context, IWebHostEnvironment hostingEnvironment, ILogger<FruityFreshContext> logger)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        // GET: Admin/AdminCustomers
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var IsCustomers = _context.Customers.AsNoTracking().OrderByDescending(
                x => x.CustomerId);
            PagedList<Customer> models = new PagedList<Customer>(IsCustomers, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Admin/AdminCustomers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Admin/AdminCustomers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminCustomers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,Username,Password,Image,Address,FullName,Email,Phone,Birthday,LastLogin,CreatedAt,Active")] 
        Customer customer, IFormFile fileThumb)
        {
            if (ModelState.IsValid)
            {
                if (fileThumb != null && fileThumb.Length > 0)
                {
                    try
                    {
                        customer.Fullname = Utilities.ToTitleCase(customer.Fullname);
                        string extension = Path.GetExtension(fileThumb.FileName);
                        string Image = Utilities.SEOUrl(customer.Fullname) + extension;

                        customer.Avatar = await Utilities.UploadFile(fileThumb, "ImageCustomer", Image.ToLower(), _hostingEnvironment);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Exception in: {ex.Message}");
                    }
                }
                else
                {
                    ModelState.AddModelError("fileThumb", "Please select a file to upload.");
                    _logger.LogInformation("Image is null or empty in file");
                    customer.Avatar = "\\Images\\ImageCustomer\\default.jpg";
                }

                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Admin/AdminCustomers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Admin/AdminCustomers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,Username,Password,Image,Address,FullName,Email,Phone,Birthday,LastLogin,CreatedAt,Active")]
        Customer customer, IFormFile fileThumb)
        {
            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCustomer = await _context.Customers.FindAsync(id);
                    if (existingCustomer == null)
                    {
                        return NotFound();
                    }
                    _context.Entry(existingCustomer).State = EntityState.Detached;
                    existingCustomer.Fullname = customer.Fullname;
                    existingCustomer.Address = customer.Address;
                    existingCustomer.Email = customer.Email;
                    existingCustomer.Phone = customer.Phone;
                    existingCustomer.Birthday = customer.Birthday;
                    if (fileThumb != null && fileThumb.Length > 0)
                    {
                        try
                        {
                            string oldImagePath = existingCustomer.Avatar;

                            if (!string.IsNullOrEmpty(oldImagePath))
                            {
                                if (System.IO.File.Exists(oldImagePath))
                                {
                                    System.IO.File.Delete(oldImagePath);
                                }
                            }
                            string extension = Path.GetExtension(fileThumb.FileName);
                            string Image = Utilities.SEOUrl(customer.Fullname) + extension;
                            existingCustomer.Avatar = await Utilities.UploadFile(fileThumb, "ImageCustomer", Image.ToLower(), _hostingEnvironment);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Exception in: {ex.Message}");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("fileThumb", "Please select a file to upload.");
                        _logger.LogInformation("Image is null or empty in file");
                        existingCustomer.Avatar = customer.Avatar;
                    }
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Admin/AdminCustomers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Admin/AdminCustomers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}
