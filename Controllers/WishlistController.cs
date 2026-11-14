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
        private readonly ReviewSummaryService _reviews;
        private readonly RecommendationService _rec;

        public WishlistController(AppDbContext db, GuestWishlistService guestWish, GuestCartService guestCart,
            ReviewSummaryService reviews, RecommendationService rec)
        {
            _db = db;
            _guestWish = guestWish;
            _guestCart = guestCart;
            _reviews = reviews;
            _rec = rec;
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
        public async Task<IActionResult> Index()
        {
            if (IsAdmin)
            {
                TempData["Error"] = "Admins do not use the wishlist.";
                return RedirectToAction("Index", "Home");
            }

            if (UserId == null)
            {
                var guest = _guestWish.Get(HttpContext.Session);
                var guestItems = HydrateGuest(guest);
                ViewBag.RatingSummaries = await _reviews.GetAsync(
                    guestItems.Where(i => i.Product != null).Select(i => i.ProductId));
                if (guestItems.Count == 0)
                    await LoadRecommendationsAsync();
                return View(guestItems);
            }

            var uid = UserId.Value;
            var items = _db.WishlistItems
                .Include(w => w.Product)
                .Where(w => w.UserId == uid)
                .OrderByDescending(w => w.AddedAt)
                .ToList();

            ViewBag.RatingSummaries = await _reviews.GetAsync(
                items.Where(i => i.Product != null).Select(i => i.ProductId));

            if (items.Count == 0)
                await LoadRecommendationsAsync();

            return View(items);
        }

        // Empty wishlist is a dead end — surface a few products to save.
        private async Task LoadRecommendationsAsync()
        {
            var recs = await _rec.GetRecommendationsAsync(UserId, 4);
            ViewBag.Recommendations = recs;
            var summaries = ViewBag.RatingSummaries as Dictionary<int, RatingSummary>
                ?? new Dictionary<int, RatingSummary>();
            foreach (var kv in await _reviews.GetAsync(recs.Select(p => p.Id)))
                summaries[kv.Key] = kv.Value;
            ViewBag.RatingSummaries = summaries;
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

        // ─── Bulk Remove ──────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BulkRemove(int[] productIds)
        {
            if (IsAdmin)
            {
                TempData["Error"] = "Admins do not use the wishlist.";
                return RedirectToAction("Index", "Home");
            }

            var ids = (productIds ?? Array.Empty<int>()).Where(i => i > 0).Distinct().ToList();
            if (ids.Count == 0)
            {
                TempData["Error"] = "Select at least one item first.";
                return RedirectToAction("Index");
            }

            if (UserId == null)
            {
                var guest = _guestWish.Get(HttpContext.Session);
                var removed = guest.RemoveAll(ids.Contains);
                _guestWish.Save(HttpContext.Session, guest);
                TempData["Success"] = removed == 1
                    ? "Removed 1 item from your wishlist."
                    : $"Removed {removed} items from your wishlist.";
                return RedirectToAction("Index");
            }

            var uid = UserId.Value;
            var items = _db.WishlistItems
                .Where(w => w.UserId == uid && ids.Contains(w.ProductId))
                .ToList();
            _db.WishlistItems.RemoveRange(items);
            _db.SaveChanges();

            TempData["Success"] = items.Count == 1
                ? "Removed 1 item from your wishlist."
                : $"Removed {items.Count} items from your wishlist.";
            return RedirectToAction("Index");
        }

        // ─── Bulk Move to Cart ────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BulkMoveToCart(int[] productIds)
        {
            if (IsAdmin)
            {
                TempData["Error"] = "Admins do not use the wishlist.";
                return RedirectToAction("Index", "Home");
            }

            var ids = (productIds ?? Array.Empty<int>()).Where(i => i > 0).Distinct().ToList();
            if (ids.Count == 0)
            {
                TempData["Error"] = "Select at least one item first.";
                return RedirectToAction("Index");
            }

            var products = _db.Products.Where(p => ids.Contains(p.Id)).ToList();

            // Variant products need an explicit size/colour choice, and sold-out items cannot move.
            var movable = products
                .Where(p => p.Stock > 0
                    && string.IsNullOrWhiteSpace(p.Sizes)
                    && string.IsNullOrWhiteSpace(p.Colors))
                .ToList();
            var needsOptions = products.Count - movable.Count;

            if (movable.Count == 0)
            {
                TempData["Error"] = needsOptions > 0
                    ? "Those items need a size or colour — open each one to choose."
                    : "The selected items are out of stock.";
                return RedirectToAction("Index");
            }

            if (UserId == null)
            {
                var guestCart = _guestCart.Get(HttpContext.Session);
                foreach (var p in movable)
                {
                    var existing = guestCart.FirstOrDefault(g => g.ProductId == p.Id);
                    if (existing != null)
                    {
                        if (existing.Quantity < p.Stock) existing.Quantity++;
                    }
                    else
                    {
                        guestCart.Add(new GuestCartItem { ProductId = p.Id, Quantity = 1, Size = "", Color = "" });
                    }
                }
                _guestCart.Save(HttpContext.Session, guestCart);

                var movedIds = movable.Select(p => p.Id).ToHashSet();
                var guestWish = _guestWish.Get(HttpContext.Session);
                guestWish.RemoveAll(movedIds.Contains);
                _guestWish.Save(HttpContext.Session, guestWish);
            }
            else
            {
                var uid = UserId.Value;
                foreach (var p in movable)
                {
                    var cartItem = _db.CartItems.FirstOrDefault(c =>
                        c.UserId == uid && c.ProductId == p.Id && c.Size == "" && c.Color == "");
                    if (cartItem != null)
                    {
                        if (cartItem.Quantity < p.Stock) cartItem.Quantity++;
                    }
                    else
                    {
                        _db.CartItems.Add(new CartItem
                        {
                            UserId = uid,
                            ProductId = p.Id,
                            Quantity = 1,
                            Size = "",
                            Color = ""
                        });
                    }
                }

                var movedIds = movable.Select(p => p.Id).ToHashSet();
                var wish = _db.WishlistItems
                    .Where(w => w.UserId == uid && movedIds.Contains(w.ProductId))
                    .ToList();
                _db.WishlistItems.RemoveRange(wish);
                _db.SaveChanges();
            }

            TempData["Success"] = movable.Count == 1
                ? "1 item moved to your cart."
                : $"{movable.Count} items moved to your cart.";
            if (needsOptions > 0)
                TempData["Error"] = needsOptions == 1
                    ? "1 item needs a size or colour, so it stayed in your wishlist."
                    : $"{needsOptions} items need a size or colour, so they stayed in your wishlist.";

            return RedirectToAction("Index", "Cart");
        }

        // ─── Move to Cart ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MoveToCart(int productId)        {
            var product = _db.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null || product.Stock <= 0)
            {
                TempData["Error"] = "Product is out of stock.";
                return RedirectToAction("Index");
            }

            // Variant products need an explicit size/color choice before they can go in the cart.
            if (!string.IsNullOrWhiteSpace(product.Sizes) || !string.IsNullOrWhiteSpace(product.Colors))
            {
                TempData["Error"] = "Please choose the available size/color options for this product first.";
                return RedirectToAction("Detail", "Products", new { id = productId });
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

        // ─── Public shared wishlist ───────────────────────────
        [HttpGet]
        public async Task<IActionResult> Shared(string? ids)
        {
            var idList = (ids ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out var n) ? n : 0)
                .Where(n => n > 0)
                .Distinct()
                .Take(50)
                .ToList();

            ViewData["Title"] = "Shared Wishlist";

            if (idList.Count == 0)
                return View(new List<Product>());

            var products = await _db.Products
                .AsNoTracking()
                .Where(p => idList.Contains(p.Id))
                .ToListAsync();

            // Preserve the order the sender saved them in.
            var ordered = idList
                .Select(fid => products.FirstOrDefault(p => p.Id == fid))
                .Where(p => p != null)
                .Select(p => p!)
                .ToList();

            ViewBag.RatingSummaries = await _reviews.GetAsync(ordered.Select(p => p.Id));
            return View(ordered);
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
