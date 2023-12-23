using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FruityFresh.Extension;
using FruityFresh.Models;
using FruityFresh.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FruityFresh.Controllers
{
    public class CartHomeController : Controller
    {
        private readonly FruityFreshContext _context;
        public CartHomeController(FruityFreshContext context)
        {
            _context = context;

        }
        public IActionResult Index()
        {
            try
            {
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
                return View(cart);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity)
        {
            try
            {
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

                if (cart != null)
                {
                    var existingItem = cart.FirstOrDefault(item => item.Products.ProductId == productId);
                    if (existingItem != null)
                    {
                        existingItem.Amount += quantity;
                    }
                    else
                    {
                        Product proId = _context.Products.SingleOrDefault(p => p.ProductId == productId);
                        cart.Add(new CartItem
                        {
                            Products = proId,
                            Amount = quantity
                        });
                    }
                    HttpContext.Session.SetObjectFromJson("Cart", cart);
                }
                var cartItemCount = cart.Sum(item => item.Amount);
                return Json(new { success = true, cartItemCount });
            }
            catch
            {
                return RedirectToAction("Error","Home");
            }
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            try
            {
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

                var existingItem = cart.FirstOrDefault(item => item.Products.ProductId == productId);
                if (existingItem != null)
                {
                    existingItem.Amount = quantity;
                    HttpContext.Session.SetObjectFromJson("Cart", cart);
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult GetCartTotal()
        {
            try
            {
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
                decimal total = cart.Sum(s => s.TotalMoney);
                return Json(total);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var itemToRemove = cart.SingleOrDefault(item => item.Products.ProductId == productId);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                HttpContext.Session.SetObjectFromJson("Cart", cart);
            }

            return RedirectToAction("Index", "CartHome");
        }
    }
}
