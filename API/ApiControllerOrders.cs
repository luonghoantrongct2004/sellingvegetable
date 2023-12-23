using FruityFresh.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FruityFresh.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiControllerOrders : ControllerBase
    {
        private readonly FruityFreshContext _context;

        public ApiControllerOrders(FruityFreshContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrder()
        {
            return await _context.Orders.ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return order;
        }
        [HttpPost]
        public async Task<IActionResult> PostOrder(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int  id, Order order)
        {
            if(id != order.OrderId)
            {
                return BadRequest();
            }
            _context.Entry(order).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }catch (DbUpdateConcurrencyException ex)
            {
                if (!OrderExist(id))
                {
                    return RedirectToAction("Error", "Home");
                }
                else
                {
                    throw ex;
                }
            }
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if(order == null) { return NotFound(); }
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        private bool OrderExist(int id)
        {
            return _context.Orders.Any(o => o.OrderId == id);
        }
    }
}
