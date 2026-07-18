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

            if (user.Balance < cartTotal)
                return CheckoutValidation.Fail(
                    $"Insufficient balance. Cart total: PKR {cartTotal:N0}, your balance: PKR {user.Balance:N0}.");

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

                foreach (var item in items)
                {
                    var product = item.Product!;
                    if (item.Quantity > product.Stock)
                        return CheckoutResult.Fail(
                            $"Not enough stock for \"{product.Name}\". Available: {product.Stock}.");

                    product.Stock -= item.Quantity;
                }

                user.Balance -= cartTotal;

                var order = new Order
                {
                    UserId = userId,
                    TotalAmount = cartTotal,
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
                        UnitPrice = item.Product!.FinalPrice
                    });
                }

                _db.Orders.Add(order);
                _db.CartItems.RemoveRange(items);
                await _db.SaveChangesAsync(cancellationToken);

                return CheckoutResult.Ok(order.Id, user.Balance);
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
        public decimal? NewBalance { get; init; }

        public static CheckoutResult Ok(int orderId, decimal newBalance) =>
            new() { Success = true, OrderId = orderId, NewBalance = newBalance };

        public static CheckoutResult Fail(string error) =>
            new() { Success = false, Error = error };
    }
}
