using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;

namespace ShoppingApp.Services
{
    public class CheckoutService
    {
        private readonly AppDbContext _db;

        public CheckoutService(AppDbContext db) => _db = db;

        public static decimal ComputeCartTotal(IEnumerable<CartItem> items) =>
            items.Sum(i => (i.Product?.FinalPrice ?? 0m) * i.Quantity);

        public static decimal ComputeDeliveryCharge(IEnumerable<CartItem> items, SiteSettings? settings)
        {
            var subtotal = ComputeCartTotal(items);
            var anyPaid = items.Any(i => i.Product != null && !i.Product.IsFreeDelivery);

            if (settings != null)
            {
                if (settings.FreeDeliveryThreshold > 0 && subtotal >= settings.FreeDeliveryThreshold)
                    return 0m;
                if (!settings.IsFreeDelivery)
                    return settings.DeliveryCharge;
            }

            if (!anyPaid)
                return 0m;

            // Max product-level charge among paid-delivery items
            return items
                .Where(i => i.Product != null && !i.Product.IsFreeDelivery)
                .Max(i => i.Product!.DeliveryCharge ?? 0m);
        }

        public CheckoutValidation Validate(
            User user,
            IReadOnlyList<CartItem> items,
            string? deliveryAddress,
            string? paymentMethod)
        {
            var cartTotal = ComputeCartTotal(items);

            if (!items.Any() || cartTotal <= 0)
                return CheckoutValidation.Fail("Your cart is empty. Add items before checkout.");

            if (string.IsNullOrWhiteSpace(deliveryAddress))
                return CheckoutValidation.Fail("Delivery address is required.");

            if (string.IsNullOrWhiteSpace(paymentMethod))
                return CheckoutValidation.Fail("Please select a payment method.");

            foreach (var item in items)
            {
                if (item.Product == null)
                    return CheckoutValidation.Fail("A product in your cart is no longer available.");

                if (item.Quantity > item.Product.Stock)
                    return CheckoutValidation.Fail(
                        $"Not enough stock for \"{item.Product.Name}\". Available: {item.Product.Stock}, in cart: {item.Quantity}.");
            }

            return CheckoutValidation.Ok(cartTotal);
        }

        public async Task<CheckoutResult> ProcessAsync(
            int userId,
            string deliveryAddress,
            string paymentMethod,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
                if (user == null)
                    return CheckoutResult.Fail("User not found.");

                var items = await _db.CartItems
                    .Include(c => c.Product)
                    .Where(c => c.UserId == userId)
                    .ToListAsync(cancellationToken);

                var validation = Validate(user, items, deliveryAddress, paymentMethod);
                if (!validation.IsValid)
                    return CheckoutResult.Fail(validation.Error!);

                var cartTotal = validation.CartTotal!.Value;
                var settings = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Id == 1, cancellationToken);
                var deliveryCharge = ComputeDeliveryCharge(items, settings);
                var grandTotal = cartTotal + deliveryCharge;

                foreach (var item in items)
                {
                    var product = item.Product!;
                    if (item.Quantity > product.Stock)
                        return CheckoutResult.Fail(
                            $"Not enough stock for \"{product.Name}\". Available: {product.Stock}.");

                    product.Stock -= item.Quantity;
                }

                var order = new Order
                {
                    UserId = userId,
                    Subtotal = cartTotal,
                    DeliveryCharge = deliveryCharge,
                    TotalAmount = grandTotal,
                    DeliveryAddress = deliveryAddress.Trim(),
                    PaymentMethod = paymentMethod.Trim(),
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow
                };

                foreach (var item in items)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product!.FinalPrice,
                        Size = item.Size ?? "",
                        Color = item.Color ?? ""
                    });
                }

                _db.Orders.Add(order);
                _db.CartItems.RemoveRange(items);
                await _db.SaveChangesAsync(cancellationToken);

                return CheckoutResult.Ok(order.Id, order.TotalAmount);
            }
            catch (Exception ex)
            {
                return CheckoutResult.Fail($"Checkout failed: {ex.Message}");
            }
        }

        public async Task<CheckoutResult> ProcessGuestAsync(
            string guestName,
            string guestPhone,
            string? guestEmail,
            string deliveryAddress,
            string paymentMethod,
            IReadOnlyList<CartItem> items,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(guestName) || string.IsNullOrWhiteSpace(guestPhone))
                    return CheckoutResult.Fail("Name and phone are required for guest checkout.");
                if (!items.Any() || items.All(i => i.Product == null))
                    return CheckoutResult.Fail("Your cart is empty. Add items before checkout.");

                foreach (var item in items)
                {
                    if (item.Product == null)
                        return CheckoutResult.Fail("A product in your cart is no longer available.");
                    if (item.Quantity > item.Product.Stock)
                        return CheckoutResult.Fail(
                            $"Not enough stock for \"{item.Product.Name}\". Available: {item.Product.Stock}.");
                }

                var cartTotal = ComputeCartTotal(items);
                var settings = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Id == 1, cancellationToken);
                var deliveryCharge = ComputeDeliveryCharge(items, settings);
                var grandTotal = cartTotal + deliveryCharge;

                foreach (var item in items)
                    item.Product!.Stock -= item.Quantity;

                var order = new Order
                {
                    UserId = null,
                    GuestName = guestName.Trim(),
                    GuestPhone = guestPhone.Trim(),
                    GuestEmail = string.IsNullOrWhiteSpace(guestEmail) ? null : guestEmail.Trim(),
                    Subtotal = cartTotal,
                    DeliveryCharge = deliveryCharge,
                    TotalAmount = grandTotal,
                    DeliveryAddress = deliveryAddress.Trim(),
                    PaymentMethod = paymentMethod.Trim(),
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow
                };

                foreach (var item in items)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product!.FinalPrice,
                        Size = item.Size ?? "",
                        Color = item.Color ?? ""
                    });
                }

                _db.Orders.Add(order);
                await _db.SaveChangesAsync(cancellationToken);

                return CheckoutResult.Ok(order.Id, order.TotalAmount);
            }
            catch (Exception ex)
            {
                return CheckoutResult.Fail($"Checkout failed: {ex.Message}");
            }
        }
    }

    public sealed class CheckoutValidation
    {
        public bool IsValid { get; init; }
        public string? Error { get; init; }
        public decimal? CartTotal { get; init; }

        public static CheckoutValidation Ok(decimal cartTotal) =>
            new() { IsValid = true, CartTotal = cartTotal };

        public static CheckoutValidation Fail(string error) =>
            new() { IsValid = false, Error = error };
    }

    public sealed class CheckoutResult
    {
        public bool Success { get; init; }
        public string? Error { get; init; }
        public int? OrderId { get; init; }
        public decimal? OrderTotal { get; init; }

        public static CheckoutResult Ok(int orderId, decimal orderTotal) =>
            new() { Success = true, OrderId = orderId, OrderTotal = orderTotal };

        public static CheckoutResult Fail(string error) =>
            new() { Success = false, Error = error };
    }
}
