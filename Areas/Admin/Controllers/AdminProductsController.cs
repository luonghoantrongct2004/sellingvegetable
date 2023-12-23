using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
//using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Controller;
using PagedList.Core;
using FruityFresh.Helper;
using FruityFresh.Models;

namespace FruityFresh.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminProductsController : Controller
    {
        private readonly FruityFreshContext _context;
        private readonly ILogger<FruityFreshContext> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public INotyfService _notyfService { get; }

        public AdminProductsController(FruityFreshContext context, ILogger<FruityFreshContext> logger, IWebHostEnvironment hostingEnvironment, INotyfService notyfService)
        {
            _context = context;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _notyfService = notyfService;
        }

        // GET: Admin/AdminProducts
        public IActionResult Index(int CategoryId = 0)
        {
            var pageNumber = int.TryParse(Request.Query["page"], out int page) ? page : 1;
            CategoryId = int.TryParse(Request.Query["CategoryId"], out int catId) ? catId : 0;
            var pageSize = 10;
            List<Product> IsProducts = new List<Product>();
            if (CategoryId != 0)
            {
                IsProducts = _context.Products
                    .AsNoTracking()
                    .Where(p => p.CategoryId == CategoryId)
                    .Include(x => x.Category)
                    .OrderByDescending(x => x.ProductId) // Sắp xếp theo giảm dần
                    .ToList();
            }
            else
            {
                IsProducts = _context.Products
                    .AsNoTracking()
                    .Include(x => x.Category)
                    .OrderByDescending(x => x.ProductId) // Sắp xếp theo giảm dần
                    .ToList();
            }

            PagedList<Product> models = new PagedList<Product>(IsProducts.AsQueryable(), pageNumber, pageSize);

            ViewBag.CurrentCategoryId = CategoryId;
            ViewBag.CurrentPage = pageNumber;
            ViewData["Category"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", CategoryId);
            return View(models);
        }

        //Admin/AdminProducts/Filter
        [ActionName("Filter")]
        [HttpGet]
        public IActionResult Filter(int CategoryId = 0)
        {
            var url = $"/Admin/AdminProducts?CategoryId={CategoryId}";
            if (CategoryId == 0)
            {
                url = $"/Admin/AdminProducts";
            }
            return Json(new { status = "success", redirectUrl = url });
        }

        // GET: Admin/AdminProducts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/AdminProducts/Create
        public IActionResult Create()
        {
            ViewData["Category"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            ViewData["Brand"] = new SelectList(_context.Brands, "BrandId", "BrandName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,Title,Description,Price,Image,Video,CategoryId,BrandId,StockQuantity,CreatedAt,DeletedAt,Active")]
        Product product,IFormFile fileThumb)
        {
            if (ModelState.IsValid)
            {
                if (fileThumb != null && fileThumb.Length > 0)
                {
                    try
                    {
                        product.ProductName = Utilities.ToTitleCase(product.ProductName);
                        string extension = Path.GetExtension(fileThumb.FileName);
                        string Image = Utilities.SEOUrl(product.ProductName) + extension;

                        product.Image = await Utilities.UploadFile(fileThumb, "ImageProducts", Image.ToLower(), _hostingEnvironment);
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
                    product.Image = "\\Images\\default.jpg";
                }

                if (string.IsNullOrEmpty(product.Image))
                {
                    _logger.LogInformation("Image is null or empty here if null or emmpty");
                    product.Image = "\\Images\\ImageProducts\\default.jpg";
                }
                product.Active = true;
                product.CreatedAt = DateTime.Now;

                _context.Add(product);
                await _context.SaveChangesAsync();
                _notyfService.Success("Create product success.");
                return RedirectToAction(nameof(Index));
            }

            ViewData["Category"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);

            return View(product);
        }

        // GET: Admin/AdminProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["Category"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["Brand"] = new SelectList(_context.Brands, "BrandId", "BrandName", product.BrandId);
            return View(product);
        }

        // POST: Admin/AdminProducts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,Title,Description,Price,Image,Video,CategoryId,BrandId,StockQuantity,CreatedAt,DeletedAt,Active")]
        Product product, IFormFile fileThumb)
		{
			if (id != product.ProductId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var existingProduct = await _context.Products.FindAsync(id);
					if (existingProduct == null)
					{
						return NotFound();
					}
                    _context.Entry(existingProduct).State = EntityState.Detached;
                    existingProduct.ProductName = product.ProductName;
                    existingProduct.Title = product.Title;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.StockQuantity = product.StockQuantity;
                    existingProduct.Discount = product.Discount;
                    if (fileThumb != null && fileThumb.Length > 0)
                    {
                        try
                        {
                            string oldImagePath = existingProduct.Image;

                            if (!string.IsNullOrEmpty(oldImagePath))
                            {
                                if (System.IO.File.Exists(oldImagePath))
                                {
                                    System.IO.File.Delete(oldImagePath);
                                }
                            }
                            string extension = Path.GetExtension(fileThumb.FileName);
                            string Image = Utilities.SEOUrl(product.ProductName) + extension;
                            existingProduct.Image = await Utilities.UploadFile(fileThumb, "ImageProducts", Image.ToLower(), _hostingEnvironment);
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
                        existingProduct.Image = product.Image;
                    }

                    if (string.IsNullOrEmpty(existingProduct.Image))
                    {
                        _logger.LogInformation("Image is null or empty here if null or emmpty");
                        existingProduct.Image = product.Image;
                    }

                    existingProduct.CreatedAt = DateTime.Now;
                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();

                    _notyfService.Success("Update product successful.");
                    return RedirectToAction(nameof(Index));
				}
				catch (Exception ex)
				{
					_logger.LogError($"Exception in Edit action: {ex.Message}");
				}
			}

			ViewData["Category"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
			return View(product);
		}


		// GET: Admin/AdminProducts/Delete/5
		public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/AdminProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            _notyfService.Success("Delete product successful.");
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
