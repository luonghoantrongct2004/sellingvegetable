using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;
using FruityFresh.Extension;
using FruityFresh.Helper;
using FruityFresh.Models;
using FruityFresh.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FruityFresh.Controllers
{
    public class CusHomeController : Controller
    {
        private readonly FruityFreshContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public INotyfService _notyfService { get; }
        public CusHomeController(FruityFreshContext context, INotyfService notyfService, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _notyfService = notyfService;
            _hostingEnvironment = hostingEnvironment;

        }

        [HttpPost]
        public IActionResult CusOrder(int customerId, string productIds, string quantities, decimal totalPrice)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // Tạo một đơn hàng mới
                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    StaticOrder = false,
                    Paid = false,
                    CustomerId = customerId,
                    PaymentDate = DateTime.Now,
                    Status = "Chưa xác nhận"
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                // Chuyển chuỗi thành mảng các productId và quantities
                var productIdArray = productIds.Split(',');
                var quantityArray = quantities.Split(',');

                // Lặp qua các sản phẩm để thêm vào đơn hàng mới
                for (int i = 0; i < productIdArray.Length; i++)
                {
                    var productId = int.Parse(productIdArray[i]);
                    var quantity = int.Parse(quantityArray[i]);

                    var orderdetail = new Orderdetail
                    {
                        ProductId = productId,
                        OrderId = order.OrderId,
                        Quantity = quantity,
                        Total = totalPrice, // Kiểm tra xem có thực sự muốn sử dụng totalPrice hay không
                        CreatedAt = DateTime.Now
                    };

                    _context.Orderdetails.Add(orderdetail);
                    _context.SaveChanges();
                    //order.OrderdetailId = orderdetail.OrderdetailId;
                }

                _context.SaveChanges();
                transaction.Commit(); // Commit transaction khi tất cả thao tác thành công
                return Json(new { success = true, redirectTo = Url.Action("OrderDetail", "CusHome", new { customerId = customerId }) });
            }
            catch (Exception ex)
            {
                transaction.Rollback(); // Rollback transaction nếu có lỗi xảy ra
                                        // Xử lý lỗi ở đây hoặc ghi log
                return Json(new { success = false, error = ex.Message });
            return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> DetailUser(int? id)
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

           HttpContext.Session.SetString("id", id.ToString());
            return View(customer);
        }
        [HttpPost]
        public async Task<IActionResult> DetailUser([Bind("FullName, Address, Email, Phone, Birthday")] Customer cus, int id, IFormFile fileThumb)
        {
            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer.Avatar == null)
            {
                return View(existingCustomer);
            }
            if (existingCustomer != null)
            {
                if (ModelState.IsValid)
                {
                    existingCustomer.Fullname = cus.Fullname;
                    existingCustomer.Address = cus.Address;
                    existingCustomer.Email = cus.Email;
                    existingCustomer.Phone = cus.Phone;
                    existingCustomer.Birthday = cus.Birthday;

                    if (fileThumb != null && fileThumb.Length > 0)
                    {
                        string oldAvatarPath = existingCustomer.Avatar;

                        if (!string.IsNullOrEmpty(oldAvatarPath))
                        {
                            string oldAvatarPathOnServer = Path.Combine("AvatarCustomer", oldAvatarPath);
                            if (System.IO.File.Exists(oldAvatarPathOnServer))
                            {
                                System.IO.File.Delete(oldAvatarPathOnServer);
                            }
                        }

                        string extension = Path.GetExtension(fileThumb.FileName);
                        string Avatar = existingCustomer.Username + extension;
                        existingCustomer.Avatar = await Utilities.UploadFile(fileThumb, "AvatarCustomer", Avatar.ToLower(), _hostingEnvironment);
                    }

                    if (string.IsNullOrEmpty(existingCustomer.Avatar))
                    {
                        existingCustomer.Avatar = "customerdefault.jpg";
                    }

                    _context.Update(existingCustomer);
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("UserAvatar", existingCustomer.Avatar);
                    HttpContext.Session.SetString("id", id.ToString());
                    _notyfService.Success("Thay đổi thành công.");
                    return View(existingCustomer);
                }
            }
            return RedirectToAction("Error", "Error");
        }
        public async Task<IActionResult> OrderDetail(int customerId)
        {
                var orderWithDetails = await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Shipper)
                .Include(o => o.Orderdetails)
              //  .ThenInclude(od => od.Product)
                .ToListAsync();

                var orderViewModel = orderWithDetails.Select(order => new OrderDetailsViewModel
                {
                    Order = order,
                    Shipper = order.Shipper,
                    OrderDetails = order.Orderdetails.ToList(),
                 //   Product = order.Orderdetails.FirstOrDefault()?.Product
                }).ToList();
                return View(orderViewModel);
        }

        [HttpGet]
        public IActionResult ChangePassword(int customerId)
        {
            var viewModel = new ChangePasswordViewModel(); // Tạo viewmodel mới để truyền vào view
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(int customerId, ChangePasswordViewModel viewmodel)
        {
            if (string.IsNullOrEmpty(viewmodel.NewPassword) || string.IsNullOrEmpty(viewmodel.OldPassword))
            {
                ViewBag.Err = "Vui lòng nhập đầy đủ mật khẩu!";
                return View(viewmodel); // Trả về view với thông báo lỗi và dữ liệu đã nhập
            }

            var user = await _context.Customers.FindAsync(customerId);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Password != viewmodel.OldPassword.ToMD5())
            {
                ViewBag.Err = "Mật khẩu cũ không chính xác!";
                return View(viewmodel); // Trả về view với thông báo lỗi và dữ liệu đã nhập
            }

            user.Password = viewmodel.NewPassword.ToMD5();
            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();

                _notyfService.Success("Đổi mật khẩu thành công!");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Err = "Có lỗi xảy ra khi cập nhật mật khẩu!";
                return View(viewmodel); // Trả về view với thông báo lỗi và dữ liệu đã nhập
            }
        }
    }
}

