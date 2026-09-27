using System.Text;
using ShoppingApp.Models;

namespace ShoppingApp.Services
{
    public static class EmailTemplates
    {
        /// <summary>
        /// Returns the Amal Collection logo URL hosted on the site itself.
        /// Gmail and most email clients block base64 data URIs, so the
        /// logo must be loaded from a real HTTP(S) URL.
        /// </summary>
        public static string GetLogoUrl(string siteUrl)
        {
            return $"{siteUrl}/images/Amal Collection-logo.png";
        }

        private static string Wrap(string title, string bodyHtml, string logoUrl, string siteUrl, string siteName = "Amal Collection")
        {
            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{title}</title>
</head>
<body style=""margin:0; padding:0; background:#f3f4f6; font-family:Arial, Helvetica, sans-serif;"">
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f3f4f6; padding:32px 16px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:560px; background:#ffffff; border-radius:14px; overflow:hidden; border:1px solid #e5e7eb; box-shadow:0 2px 8px rgba(0,0,0,0.05);"">
                    <!-- Header / Branding -->
                    <tr>
                        <td align=""center"" style=""background:#171e30; padding:32px 24px 28px;"">
                            <img src=""{logoUrl}"" alt=""{siteName}"" width=""72"" height=""72"" style=""height:72px; width:72px; border-radius:14px; object-fit:cover; display:inline-block;"" onerror=""this.style.display='none'"">
                            <div style=""color:#ffffff; font-size:24px; font-weight:bold; letter-spacing:0.5px; margin-top:10px;"">{siteName}</div>
                            <div style=""color:#9ca3af; font-size:12px; letter-spacing:2px; text-transform:uppercase; margin-top:4px;"">Shop Smart. Live Better.</div>
                        </td>
                    </tr>
                    <!-- Body -->
                    <tr>
                        <td style=""padding:32px 28px;"" align=""left"">
                            <h1 style=""font-size:20px; color:#111827; margin:0 0 16px;"">{title}</h1>
                            {bodyHtml}
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td align=""center"" style=""padding:20px 24px; background:#f9fafb; border-top:1px solid #e5e7eb;"">
                            <a href=""{siteUrl}"" style=""color:#7c5cfc; font-size:13px; font-weight:bold; text-decoration:none;"">{siteName}</a>
                            <div style=""font-size:11px; color:#9ca3af; margin-top:6px;"">This is an automated message from the {siteName} website.</div>
                            <div style=""font-size:11px; color:#9ca3af; margin-top:2px;"">&copy; {DateTime.UtcNow.Year} {siteName}. All rights reserved.</div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        public static string ResetOtp(string userName, string otp, string logoUrl, string siteUrl)
        {
            var body = $@"<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">Hello {userName},</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">We received a request to reset your <strong>Amal Collection</strong> account password. Use the verification code below to continue:</p>
<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""margin:0 0 18px;"">
    <tr>
        <td align=""center"" style=""background:#f8fafc; border:2px dashed #7c5cfc; border-radius:10px; padding:20px;"">
            <div style=""font-size:11px; letter-spacing:2px; text-transform:uppercase; color:#6b7280; margin-bottom:6px;"">Verification Code</div>
            <div style=""font-size:32px; font-weight:bold; letter-spacing:8px; color:#171e30; font-family:Consolas, monospace;"">{otp}</div>
        </td>
    </tr>
</table>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 8px;"">This code is valid for <strong>10 minutes</strong>. If you did not request this, you can safely ignore this email.</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:20px 0 0;"">Regards,<br><strong>Amal Collection Security Team</strong></p>";

            return Wrap("Password Reset Verification", body, logoUrl, siteUrl);
        }

        public static string PasswordResetConfirmation(string userName, string logoUrl, string siteUrl)
        {
            var body = $@"<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">Hello {userName},</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">This is to confirm that your <strong>{siteUrl}</strong> account password was successfully changed.</p>
<table role=""presentation"" style=""background:#f0fdf4; border:1px solid #bbf7d0; border-radius:8px; padding:14px 18px; margin:0 0 18px; width:100%;"">
    <tr>
        <td style=""font-size:13px; color:#166534; line-height:1.6;"">&#10003; Your password was updated on {DateTime.UtcNow.AddHours(5).ToString("dd MMM yyyy 'at' hh:mm tt")} (PKT)</td>
    </tr>
</table>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 8px;"">If you made this change, no further action is required.</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 8px;"">If you did <strong>not</strong> request this change, please secure your account immediately by resetting your password and contacting our support team.</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:20px 0 0;"">Regards,<br><strong>Amal Collection Security Team</strong></p>";

            return Wrap("Password Reset Confirmation", body, logoUrl, siteUrl);
        }

        public static string Welcome(string userName, string logoUrl, string siteUrl)
        {
            var body = $@"<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">Welcome to <strong>{siteUrl}</strong>, {userName}!</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">Your account has been created successfully. You can now browse thousands of products, add items to your wishlist, and enjoy a smooth shopping experience.</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 8px;"">If you have any questions, our support team is always happy to help.</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:20px 0 0;"">Happy shopping!<br><strong>Amal Collection Team</strong></p>";

            return Wrap("Welcome to Amal Collection", body, logoUrl, siteUrl);
        }

        // ─── Order helpers ────────────────────────────────────────

        private static string Pkt(DateTime utc) => utc.AddHours(5).ToString("dd MMM yyyy 'at' hh:mm tt") + " (PKT)";

        private static string ItemRowsHtml(Order order)
        {
            var sb = new StringBuilder();
            foreach (var it in order.OrderItems)
            {
                var name = it.Product?.Name ?? $"Product #{it.ProductId}";
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(it.Size)) parts.Add(it.Size);
                if (!string.IsNullOrWhiteSpace(it.Color)) parts.Add(it.Color);
                var variantHtml = parts.Count > 0
                    ? $"<div style=\"font-size:11px; color:#6b7280; margin-top:2px;\">{string.Join(" &middot; ", parts)}</div>"
                    : "";
                var lineTotal = it.UnitPrice * it.Quantity;
                sb.Append($@"
        <tr>
            <td style=""padding:9px 12px; font-size:13px; color:#111827; border-top:1px solid #e5e7eb;"">{name}{variantHtml}</td>
            <td style=""padding:9px 12px; font-size:13px; color:#374151; border-top:1px solid #e5e7eb; text-align:center;"">{it.Quantity}</td>
            <td style=""padding:9px 12px; font-size:13px; color:#374151; border-top:1px solid #e5e7eb; text-align:right;"">Rs. {it.UnitPrice:N0}</td>
            <td style=""padding:9px 12px; font-size:13px; color:#111827; border-top:1px solid #e5e7eb; text-align:right;"">Rs. {lineTotal:N0}</td>
        </tr>");
            }
            return sb.ToString();
        }

        private static string ItemsTableHtml(Order order)
        {
            return $@"
<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""border:1px solid #e5e7eb; border-radius:8px; overflow:hidden; margin:0 0 16px;"">
    <tr style=""background:#f8fafc;"">
        <th style=""padding:9px 12px; font-size:11px; text-transform:uppercase; letter-spacing:1px; color:#6b7280; text-align:left;"">Item</th>
        <th style=""padding:9px 12px; font-size:11px; text-transform:uppercase; letter-spacing:1px; color:#6b7280; text-align:center;"">Qty</th>
        <th style=""padding:9px 12px; font-size:11px; text-transform:uppercase; letter-spacing:1px; color:#6b7280; text-align:right;"">Price</th>
        <th style=""padding:9px 12px; font-size:11px; text-transform:uppercase; letter-spacing:1px; color:#6b7280; text-align:right;"">Total</th>
    </tr>{ItemRowsHtml(order)}
</table>";
        }

        private static string TotalsHtml(Order order)
        {
            var delivery = order.DeliveryCharge > 0
                ? $@"
    <tr>
        <td style=""padding:5px 0; font-size:13px; color:#374151;"">Delivery</td>
        <td style=""padding:5px 0; font-size:13px; color:#111827; text-align:right;"">Rs. {order.DeliveryCharge:N0}</td>
    </tr>"
                : "";
            return $@"
<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""margin:0 0 16px;"">
    <tr>
        <td style=""padding:5px 0; font-size:13px; color:#374151;"">Subtotal</td>
        <td style=""padding:5px 0; font-size:13px; color:#111827; text-align:right;"">Rs. {order.Subtotal:N0}</td>
    </tr>{delivery}
    <tr>
        <td style=""padding:8px 0 0; font-size:15px; font-weight:bold; color:#111827; border-top:1px solid #e5e7eb;"">Total</td>
        <td style=""padding:8px 0 0; font-size:15px; font-weight:bold; color:#111827; text-align:right; border-top:1px solid #e5e7eb;"">Rs. {order.TotalAmount:N0}</td>
    </tr>
</table>";
        }

        private static string MetaHtml(Order order)
        {
            var address = string.IsNullOrWhiteSpace(order.DeliveryAddress) ? "—" : order.DeliveryAddress;
            var payment = string.IsNullOrWhiteSpace(order.PaymentMethod) ? "—" : order.PaymentMethod;
            return $@"
<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background:#f8fafc; border:1px solid #e5e7eb; border-radius:8px; padding:14px 18px; margin:0 0 16px; width:100%;"">
    <tr>
        <td style=""font-size:13px; color:#374151; padding:4px 0;""><strong>Order Number:</strong></td>
        <td style=""font-size:13px; color:#111827; padding:4px 0; text-align:right;"">#{order.Id}</td>
    </tr>
    <tr>
        <td style=""font-size:13px; color:#374151; padding:4px 0;""><strong>Placed On:</strong></td>
        <td style=""font-size:13px; color:#111827; padding:4px 0; text-align:right;"">{Pkt(order.CreatedAt)}</td>
    </tr>
    <tr>
        <td style=""font-size:13px; color:#374151; padding:4px 0;""><strong>Payment Method:</strong></td>
        <td style=""font-size:13px; color:#111827; padding:4px 0; text-align:right;"">{payment}</td>
    </tr>
    <tr>
        <td style=""font-size:13px; color:#374151; padding:4px 0;""><strong>Delivery Address:</strong></td>
        <td style=""font-size:13px; color:#111827; padding:4px 0; text-align:right;"">{address}</td>
    </tr>
</table>";
        }

        public static string NewRegistration(string userName, string userEmail, string logoUrl, string siteUrl)
        {
            var body = $@"<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">A new user just registered on Amal Collection.</p>
<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background:#f8fafc; border:1px solid #e5e7eb; border-radius:8px; padding:14px 18px; margin:0 0 16px; width:100%;"">
    <tr>
        <td style=""font-size:13px; color:#374151; padding:4px 0;""><strong>Name:</strong></td>
        <td style=""font-size:13px; color:#111827; padding:4px 0; text-align:right;"">{userName}</td>
    </tr>
    <tr>
        <td style=""font-size:13px; color:#374151; padding:4px 0;""><strong>Email:</strong></td>
        <td style=""font-size:13px; color:#111827; padding:4px 0; text-align:right;"">{userEmail}</td>
    </tr>
    <tr>
        <td style=""font-size:13px; color:#374151; padding:4px 0;""><strong>Time:</strong></td>
        <td style=""font-size:13px; color:#111827; padding:4px 0; text-align:right;"">{DateTime.UtcNow.AddHours(5).ToString("dd MMM yyyy 'at' hh:mm tt")} (PKT)</td>
    </tr>
</table>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:20px 0 0;"">Regards,<br><strong>Amal Collection System</strong></p>";

            return Wrap("New User Registered", body, logoUrl, siteUrl);
        }

        // ─── Order emails ────────────────────────────────────────

        public static string OrderConfirmation(string userName, Order order, string logoUrl, string siteUrl)
        {
            var body = $@"<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">Dear {userName},</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">Thank you for shopping with Amal Collection! Your order has been placed successfully. Here are your order details:</p>
{MetaHtml(order)}
{ItemsTableHtml(order)}
{TotalsHtml(order)}
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 8px;"">We will notify you as soon as your order status changes. You can also track it anytime using your order number — no login needed.</p>
<p style=""margin:0 0 8px;""><a href=""{siteUrl}/Orders/Track?id={order.Id}"" style=""display:inline-block; background:#171e30; color:#ffffff; font-size:13px; font-weight:bold; text-decoration:none; padding:11px 22px; border-radius:8px;"">Track Your Order</a></p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:20px 0 0;"">Thank you for trusting us!<br><strong>Amal Collection Team</strong></p>";

            return Wrap("Order Confirmation #" + order.Id, body, logoUrl, siteUrl);
        }

        public static string NewOrder(string userName, Order order, string logoUrl, string siteUrl)
        {
            var customerEmail = string.IsNullOrWhiteSpace(order.GuestEmail) ? order.User?.Email ?? "—" : order.GuestEmail;
            var customerPhone = string.IsNullOrWhiteSpace(order.GuestPhone) ? order.User?.Phone ?? "—" : order.GuestPhone;
            var body = $@"<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">A new order has been placed on Amal Collection.</p>
<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background:#f8fafc; border:1px solid #e5e7eb; border-radius:8px; padding:14px 18px; margin:0 0 16px; width:100%;"">
    <tr>
        <td style=""font-size:13px; color:#374151; padding:4px 0;""><strong>Customer:</strong></td>
        <td style=""font-size:13px; color:#111827; padding:4px 0; text-align:right;"">{userName}</td>
    </tr>
    <tr>
        <td style=""font-size:13px; color:#374151; padding:4px 0;""><strong>Email:</strong></td>
        <td style=""font-size:13px; color:#111827; padding:4px 0; text-align:right;"">{customerEmail}</td>
    </tr>
    <tr>
        <td style=""font-size:13px; color:#374151; padding:4px 0;""><strong>Phone:</strong></td>
        <td style=""font-size:13px; color:#111827; padding:4px 0; text-align:right;"">{customerPhone}</td>
    </tr>
</table>
{MetaHtml(order)}
{ItemsTableHtml(order)}
{TotalsHtml(order)}
<p style=""margin:0 0 8px;""><a href=""{siteUrl}/Admin/Orders"" style=""display:inline-block; background:#171e30; color:#ffffff; font-size:13px; font-weight:bold; text-decoration:none; padding:11px 22px; border-radius:8px;"">View Order in Admin</a></p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:20px 0 0;"">Regards,<br><strong>Amal Collection System</strong></p>";

            return Wrap("New Order #" + order.Id, body, logoUrl, siteUrl);
        }

        public static string OrderStatusUpdate(string userName, Order order, string logoUrl, string siteUrl)
        {
            var (message, tone) = order.Status switch
            {
                "confirmed" => ("Your order has been confirmed and is now being prepared for dispatch.", "#f0fdf4"),
                "shipped"   => ("Good news! Your order is on its way and should arrive soon.", "#eff6ff"),
                "delivered" => ("Your order has been delivered. We hope you love your purchase! If anything is not right, a 7-day exchange is available.", "#f0fdf4"),
                "cancelled" => ("Your order has been cancelled. If this was a mistake or you need any help, please contact us — we are happy to assist.", "#fef2f2"),
                _           => ($"Your order status has been updated to: {order.Status}.", "#f8fafc")
            };

            var body = $@"<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">Hello {userName},</p>
<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background:{tone}; border:1px solid #e5e7eb; border-radius:8px; padding:16px 18px; margin:0 0 16px; width:100%;"">
    <tr>
        <td style=""font-size:14px; color:#111827; line-height:1.7;"">{message}</td>
    </tr>
</table>
{MetaHtml(order)}
{TotalsHtml(order)}
<p style=""margin:16px 0 8px;""><a href=""{siteUrl}/Orders/Track?id={order.Id}"" style=""display:inline-block; background:#171e30; color:#ffffff; font-size:13px; font-weight:bold; text-decoration:none; padding:11px 22px; border-radius:8px;"">Track Your Order</a></p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 8px;"">Need to change something? Contact us any time — we are happy to help.</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:20px 0 0;"">Regards,<br><strong>Amal Collection Team</strong></p>";

            return Wrap("Order #" + order.Id + " Update", body, logoUrl, siteUrl);
        }
    }
}
