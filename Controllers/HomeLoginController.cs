using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FruityFresh.Extension;
using FruityFresh.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using FruityFresh.ViewModel;

namespace FruityFresh.Controllers
{
    [Authorize]
    public class HomeLoginController : Controller
    {
        private readonly FruityFreshContext _context;
        public INotyfService _notyfService { get; }
        public HomeLoginController(FruityFreshContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;

        }
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password, Customer cus)
        {
            HttpContext.Session.Clear();
            if (string.IsNullOrEmpty(cus.Username) || string.IsNullOrEmpty(cus.Password))
            {
                return View(cus);
            }
            var user = _context.Customers.AsNoTracking().FirstOrDefault(u => u.Username == username);
            var admin = _context.AdminAccounts.FirstOrDefault(a => a.Username == username);

            try
            {
                if (admin != null && admin.Password == password)
                {
                    HttpContext.Session.SetString("Admin", admin.FullName);
                  //  HttpContext.Session.SetString("AdminAvatar", (string)admin.);

                    _notyfService.Success("Chào mừng bro " + admin.FullName + " đến admin.");
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }
                if (user == null) { return View(); }
                string pass = password.ToMD5();
                if (pass != user.Password)
                {
                    ViewBag.Err = "Nhập sai mật khẩu rùi.";
                    return View();
                }
                //if (user.Active == false) { return RedirectToAction("Notificate", "Login"); }

                if (user == null) { return RedirectToAction("SignUp", "HomeLogin"); }

                //save session
                HttpContext.Session.SetString("CusId", user.CustomerId.ToString());
                HttpContext.Session.SetString("Customer", user.Fullname.ToString());
                HttpContext.Session.SetString("UserAvatar", user.Avatar);
                var cusId = HttpContext.Session.GetString("CusId");
                var accAvatar = HttpContext.Session.GetString("UserAvatar");
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Fullname),
                    new Claim("CusId", user.CustomerId.ToString())
                };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);
                _notyfService.Success("Đăng Nhập Oke Rồi Nha " + user.Fullname);
                return RedirectToAction("Index", "Home");

            }
            catch
            {
                return RedirectToAction("SignUp", "HomeLogin");
            }
        }


        [AllowAnonymous]
        public async Task<IActionResult> SignUp(Customer cus)
        {
            if (string.IsNullOrEmpty(cus.Username) || string.IsNullOrEmpty(cus.Password))
            {
                return View(cus);
            }

            var accountUser = _context.Customers.FirstOrDefault(u => u.Username == cus.Username);
            var accountAdmin = _context.AdminAccounts.FirstOrDefault(a => a.Username == cus.Username);

            if (accountUser != null || accountAdmin != null)
            {
                ViewBag.Err = "Tên người dùng đã tồn tại!";
                return View(cus);
            }
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    string pass = cus.Password.ToMD5();
                    Customer newCustomer = new Customer
                    {
                        Fullname = cus.Fullname,
                        Username = cus.Username,
                        Password = pass, // Set password
                        CreatedAt = DateTime.Now,
                        Avatar = "\\Avatars\\AvatarCustomer\\default.jpg"
                    };

                    _context.Add(newCustomer);
                    await _context.SaveChangesAsync();

                    // Commit the transaction if everything succeeds
                    await transaction.CommitAsync();

                    // Now set session and sign-in after the successful completion of SaveChangesAsync
                    HttpContext.Session.SetString("CusId", newCustomer.CustomerId.ToString());
                    HttpContext.Session.SetString("Customer", newCustomer.Fullname);
                    HttpContext.Session.SetString("UserAvatar", newCustomer.Avatar);

                    var cusId = HttpContext.Session.GetString("UserId");
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, newCustomer.Fullname),
                        new Claim("UserId", newCustomer.CustomerId.ToString())
                        // Add other necessary claims if required
                    };

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(claimsPrincipal);

                    _notyfService.Success("Đăng Ký Chuẩn Rồi Nha " + newCustomer.Fullname);
                    return RedirectToAction("Index", "Home");
                    }
                    catch (Exception ex)
                    {
                    await transaction.RollbackAsync();
                    ViewBag.Err = "Có lỗi xảy ra khi đăng ký!";
                    return View(cus);
                }
            }

        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("Username");
            HttpContext.Session.Remove("UserAvatar");
            HttpContext.Session.Remove("Admin");
            HttpContext.Session.Remove("AdminAvatar");

            // Thêm các header để yêu cầu trình duyệt xóa thông tin đăng nhập
            Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1
            Response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0
            Response.Headers.Add("Expires", "0"); // Proxies
            Response.Headers.Add("Set-Cookie", "HttpOnly; Secure; SameSite=Strict; Max-Age=0"); // Xóa cookie nếu có

            return RedirectToAction("Login");
        }

        public IActionResult ValidatePhone(string phone)
        {
            try
            {
                var cus = _context.Customers.SingleOrDefault(c => c.Phone.ToLower() == phone.ToLower());
                if (cus != null)
                {
                    return Json(data: "Số điện thoại " + phone + " Đã được sử dụng rùi.");
                }
                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }


    }
}

