using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;
using ShoppingApp.Services;

namespace ShoppingApp.Controllers
{
    public class WishlistController : Controller
    {
        private readonly AppDbContext _db;
        private readonly GuestWishlistService _guestWish;
        private readonly GuestCartService _guestCart;

        public WishlistController(AppDbContext db, GuestWishlistService guestWish, GuestCartService guestCart)
        {
            _db = db;
            _guestWish = guestWish;
            _guestCart = guestCart;
        }

        private int? UserId => HttpContext.Session.GetInt32("UserId");
        private bool IsAdmin => HttpContext.Session.GetString("UserRole") == "admin";

        private List<WishlistItem> HydrateGuest(List<int> productIds)
        {
            if (productIds.Count == 0) return new List<WishlistItem>();
            var products = _db.Products.Where(p => productIds.Contains(p.Id)).ToList();
            var items = new List<WishlistItem>();
            foreach (var id in productIds)
            {
                var p = products.FirstOrDefault(x => x.Id == id);
                if (p == null) continue;
                items.Add(new WishlistItem
                {
                    Id = -id,
                    UserId = -1,
                    ProductId = id,
                    AddedAt = DateTime.UtcNow,
                    Product = p
                });
            }
            return items;
        }

        // ─── View Wishlist ────────────────────────────────────
        public IActionResult Index()
        {
            if (IsAdmin)
            {
                TempData["Error"] = "Admins do not use the wishlist.";
                return RedirectToAction("Index", "Home");
            }

            if (UserId == null)
            {
                var guest = _guestWish.Get(HttpContext.Session);
                return View(HydrateGuest(guest));
            }

            var uid = UserId.Value;
            var items = _db.WishlistItems
                .Include(w => w.Product)
                .Where(w => w.UserId == uid)
                .OrderByDescending(w => w.AddedAt)
                .ToList();

            return View(items);
        }

        // ─── Toggle (Add/Remove) ──────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle(int productId, string? returnUrl = null)
        {
            if (IsAdmin)
            {
                TempData["Error"] = "Admins do not use the wishlist.";
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                return RedirectToAction("Detail", "Products", new { id = productId });
            }

            if (UserId == null)
            {
                var guest = _guestWish.Get(HttpContext.Session);
                if (guest.Contains(productId))
                {
                    guest.Remove(productId);
                    TempData["WishlistMsg"] = "removed";
                }
                else
                {
                    guest.Add(productId);
                    TempData["WishlistMsg"] = "added";
                }
                _guestWish.Save(HttpContext.Session, guest);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Detail", "Products", new { id = productId });
            }

            var uid = UserId.Value;
            var existing = _db.WishlistItems
                .FirstOrDefault(w => w.UserId == uid && w.ProductId == productId);

            if (existing != null)
            {
                _db.WishlistItems.Remove(existing);
                TempData["WishlistMsg"] = "removed";
            }
            else
            {
                _db.WishlistItems.Add(new WishlistItem
                {
                    UserId    = uid,
                    ProductId = productId,
                    AddedAt   = DateTime.UtcNow
                });
                TempData["WishlistMsg"] = "added";
            }

            _db.SaveChanges();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        // ─── Remove From Wishlist ─────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            if (UserId == null)
            {
                var guest = _guestWish.Get(HttpContext.Session);
                guest.Remove(productId);
                _guestWish.Save(HttpContext.Session, guest);
                TempData["Success"] = "Removed from wishlist.";
                return RedirectToAction("Index");
            }

            var uid = UserId.Value;
            var item = _db.WishlistItems.FirstOrDefault(w => w.UserId == uid && w.ProductId == productId);
            if (item != null) { _db.WishlistItems.Remove(item); _db.SaveChanges(); }
            TempData["Success"] = "Removed from wishlist.";
            return RedirectToAction("Index");
        }

        // ─── Move to Cart ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MoveToCart(int productId)
        {
            var product = _db.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null || product.Stock <= 0)
            {
                TempData["Error"] = "Product is out of stock.";
                return RedirectToAction("Index");
            }

            if (UserId == null)
            {
                var guestWish = _guestWish.Get(HttpContext.Session);
                var guestCart = _guestCart.Get(HttpContext.Session);
                var existing = guestCart.FirstOrDefault(g => g.ProductId == productId);
                if (existing != null) existing.Quantity++;
                else guestCart.Add(new GuestCartItem { ProductId = productId, Quantity = 1, Size = "", Color = "" });
                guestWish.Remove(productId);
                _guestCart.Save(HttpContext.Session, guestCart);
                _guestWish.Save(HttpContext.Session, guestWish);
                TempData["Success"] = $"{product.Name} moved to cart!";
                return RedirectToAction("Index", "Cart");
            }

            var uid = UserId.Value;
            var cartItem = _db.CartItems.FirstOrDefault(c => c.UserId == uid && c.ProductId == productId && c.Size == "" && c.Color == "");
            if (cartItem != null) cartItem.Quantity++;
            else _db.CartItems.Add(new CartItem { UserId = uid, ProductId = productId, Quantity = 1, Size = "", Color = "" });

            var wish = _db.WishlistItems.FirstOrDefault(w => w.UserId == uid && w.ProductId == productId);
            if (wish != null) _db.WishlistItems.Remove(wish);

            _db.SaveChanges();
            TempData["Success"] = $"{product.Name} moved to cart!";
            return RedirectToAction("Index", "Cart");
        }

        // ─── Count (for badge) ────────────────────────────────
        public IActionResult Count()
        {
            if (UserId == null) return Json(_guestWish.Count(HttpContext.Session));
            var uid = UserId.Value;
            var count = _db.WishlistItems.Count(w => w.UserId == uid);
            return Json(count);
        }
    }
}
