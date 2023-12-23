using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FruityFresh.Models;
using System.Collections.Generic;
using System.Linq;

namespace FruityFresh.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SearchController : Controller
    {
        private readonly FruityFreshContext _context;
        public SearchController(FruityFreshContext context)
        {
            _context = context;
        }
        [HttpPost]
        [ActionName("FindProduct")]
        public IActionResult FindProduct(string keyword)
        {
            List<Product> ls = new List<Product>();
            if (string.IsNullOrEmpty(keyword) || keyword.Length < 1)
            {
                return PartialView("ListProductSearchPartial", null);
            }
            ls = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.ProductName.Contains(keyword))
                .OrderByDescending(x => x.ProductName)
                .Take(10).ToList();

            return PartialView("ListProductSearchPartial", ls != null ? ls : null);
        }

    }
}