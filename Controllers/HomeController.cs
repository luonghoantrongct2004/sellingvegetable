using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PagedList.Core;
using FruityFresh.Extension;
using FruityFresh.Models;
using FruityFresh.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FruityFresh.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
        private readonly FruityFreshContext _context;

        public HomeController(ILogger<HomeController> logger, FruityFreshContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(string keyword = null)
        {
            var pageNumber = int.TryParse(Request.Query["page"], out int page) ? page : 1;
            var pageSize = 10;
            var totalProducts = 0;
            List<Product> products;

            if (!string.IsNullOrEmpty(keyword))
            {
                products = _context.Products
                                    .Where(p => p.ProductName.Contains(keyword)) // Lọc theo từ khóa tìm kiếm
                                    .OrderBy(p => p.ProductId)
                                    .Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();

                totalProducts = _context.Products.Count(p => p.ProductName.Contains(keyword)); // Đếm số sản phẩm sau khi lọc
            }
            else
            {
                products = _context.Products
                                    .OrderBy(p => p.ProductId)
                                    .Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();

                totalProducts = _context.Products.Count(); // Số lượng tổng nếu không có tìm kiếm
            }

            var models = new StaticPagedList<Product>(products, pageNumber, pageSize, totalProducts);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.Keyword = keyword; // Truyền từ khóa tìm kiếm đến view

            return View(models);
        }

        public IActionResult Details(int id)
        {
            try
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
                /*if (product == null)
                {
                    return RedirectToAction("Index");
                }
                var lsProduct = _context.Products.AsNoTracking()
                    .Where(p => p.ProductId != id && p.Active == true)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(10).ToList();
                List<Product> products = _context.Products.ToList();
                ViewBag.Products = lsProduct;
                ViewBag.SingleProduct = product;*/
                return View(product);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }
		public IActionResult About()
		{
			return View();
		}

		public IActionResult Error()
        {
            return View();
        }

       /* [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
            return RedirectToAction("Error", "Home");
			//return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}*/
	}
}
