using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;

namespace ShoppingApp.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _db;
        public OrdersController(AppDbContext db) { _db = db; }

        private int? UserId => HttpContext.Session.GetInt32("UserId");

        // ─── Checkout GET ─────────────────────────────────────────
        [HttpGet]
        public IActionResult Checkout()
        {
            if (UserId == null) return RedirectToAction("Login", "Auth");
            var items = _db.CartItems.Include(c => c.Product).Where(c => c.UserId == UserId).ToList();
            if (!items.Any()) return RedirectToAction("Index", "Cart");
            ViewBag.Total = items.Sum(i => i.Product!.FinalPrice * i.Quantity);
            return View(items);
        }

        // ─── Checkout POST ────────────────────────────────────────
        [HttpPost]
        public IActionResult Checkout(string deliveryAddress, string paymentMethod)
        {
            if (UserId == null) return RedirectToAction("Login", "Auth");

            var items = _db.CartItems.Include(c => c.Product).Where(c => c.UserId == UserId).ToList();
            if (!items.Any()) return RedirectToAction("Index", "Cart");

            if (string.IsNullOrWhiteSpace(deliveryAddress))
            { TempData["Error"] = "Delivery address is required."; return RedirectToAction("Checkout"); }

            decimal total = items.Sum(i => i.Product!.FinalPrice * i.Quantity);

            var order = new Order
            {
                UserId = UserId.Value,
                TotalAmount = total,
                DeliveryAddress = deliveryAddress,
                PaymentMethod = paymentMethod,
                Status = "pending"
            };
            _db.Orders.Add(order);
            _db.SaveChanges();

            foreach (var item in items)
            {
                _db.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product!.FinalPrice
                });
                // Reduce stock
                if (item.Product != null) item.Product.Stock -= item.Quantity;
            }

            // Clear cart
            _db.CartItems.RemoveRange(items);
            _db.SaveChanges();

            TempData["OrderId"] = order.Id;
            return RedirectToAction("Confirmation");
        }

        // ─── Order Confirmation ──────────────────────────────────
        public IActionResult Confirmation()
        {
            ViewBag.OrderId = TempData["OrderId"];
            return View();
        }

        // ─── Order History ────────────────────────────────────────
        public IActionResult History()
        {
            if (UserId == null) return RedirectToAction("Login", "Auth");

            var orders = _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == UserId)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();
            return View(orders);
        }
    }
}
