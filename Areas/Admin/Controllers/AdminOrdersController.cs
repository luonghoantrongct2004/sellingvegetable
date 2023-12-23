using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FruityFresh.Models;

namespace FruityFresh.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly FruityFreshContext _context;

        public AdminOrdersController(FruityFreshContext context)
        {
            _context = context;
        }

        // GET: Admin/AdminOrders
        public async Task<IActionResult> Index()
        {
            var fruityFreshContext = _context.Orders.Include(o => o.Customer).Include(o => o.Orderdetail).Include(o => o.Shipper);
            return View(await fruityFreshContext.ToListAsync());
        }

        // GET: Admin/AdminOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Orderdetail)
                .Include(o => o.Shipper)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Admin/AdminOrders/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId");
            ViewData["OrderdetailId"] = new SelectList(_context.Orderdetails, "OrderdetailId", "OrderdetailName");
            ViewData["ShipperId"] = new SelectList(_context.Shippers, "ShipperId", "ShipperName");
            return View();
        }

        // POST: Admin/AdminOrders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,CustomerId,OrderDate,ShipDate,StaticOrder,Paid,PaymentDate,PaymentId,Note,CreatedAt,ShipperId,Status,OrderdetailId")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
            ViewData["OrderdetailId"] = new SelectList(_context.Orderdetails, "OrderdetailId", "OrderdetailId", order.OrderdetailId);
            ViewData["ShipperId"] = new SelectList(_context.Shippers, "ShipperId", "ShipperId", order.ShipperId);
            return View(order);
        }

        // GET: Admin/AdminOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
            ViewData["OrderdetailId"] = new SelectList(_context.Orderdetails, "OrderdetailId", "OrderdetailId", order.OrderdetailId);
            ViewData["ShipperId"] = new SelectList(_context.Shippers, "ShipperId", "ShipperName", order.Shipper.ShipperName);
            return View(order);
        }

        // POST: Admin/AdminOrders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,CustomerId,OrderDate,ShipDate,StaticOrder,Paid,PaymentDate,PaymentId,Note,CreatedAt,ShipperId,Status,OrderdetailId")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
            ViewData["OrderdetailId"] = new SelectList(_context.Orderdetails, "OrderdetailId", "OrderdetailId", order.OrderdetailId);
            ViewData["ShipperId"] = new SelectList(_context.Shippers, "ShipperId", "ShipperId", order.ShipperId);
            return View(order);
        }

        // GET: Admin/AdminOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Orderdetail)
                .Include(o => o.Shipper)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/AdminOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }

        public IActionResult ConfirmOrder(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == id);
            order.Status = "Đã xác nhận";
            _context.Update(order);
            _context.SaveChanges();
            return RedirectToAction("Index", "AdminOrders");
        }
        public IActionResult RejectOrder(int id)
        {
            var order = _context.Orders.FirstOrDefault(e => e.OrderId == id);
            order.Status = "Đơn hàng bị từ chối";
            _context.Update(order);
            _context.SaveChanges();
            return RedirectToAction("Index", "AdminOrders");
        }
    }
}
