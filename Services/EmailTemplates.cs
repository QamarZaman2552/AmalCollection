namespace ShoppingApp.Services
{
    public static class EmailTemplates
    {
        /// <summary>
        /// Returns the BaazWix logo URL hosted on the site itself.
        /// Gmail and most email clients block base64 data URIs, so the
        /// logo must be loaded from a real HTTP(S) URL.
        /// </summary>
        public static string GetLogoUrl(string siteUrl)
        {
            return $"{siteUrl}/images/baazwix-logo.png";
        }

        private static string Wrap(string title, string bodyHtml, string logoUrl, string siteUrl, string siteName = "BaazWix")
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
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">We received a request to reset your <strong>BaazWix</strong> account password. Use the verification code below to continue:</p>
<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""margin:0 0 18px;"">
    <tr>
        <td align=""center"" style=""background:#f8fafc; border:2px dashed #7c5cfc; border-radius:10px; padding:20px;"">
            <div style=""font-size:11px; letter-spacing:2px; text-transform:uppercase; color:#6b7280; margin-bottom:6px;"">Verification Code</div>
            <div style=""font-size:32px; font-weight:bold; letter-spacing:8px; color:#171e30; font-family:Consolas, monospace;"">{otp}</div>
        </td>
    </tr>
</table>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 8px;"">This code is valid for <strong>10 minutes</strong>. If you did not request this, you can safely ignore this email.</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:20px 0 0;"">Regards,<br><strong>BaazWix Security Team</strong></p>";

            return Wrap("Password Reset Verification", body, logoUrl, siteUrl);
        }

        public static string PasswordResetConfirmation(string userName, string logoUrl, string siteUrl)
        {
            var body = $@"<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">Hello {userName},</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">This is to confirm that your <strong>{siteUrl}</strong> account password was successfully changed.</p>
<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""background:#f0fdf4; border:1px solid #bbf7d0; border-radius:8px; padding:14px 18px; margin:0 0 18px; width:100%;"">
    <tr>
        <td style=""font-size:13px; color:#166534; line-height:1.6;"">&#10003; Your password was updated on {DateTime.UtcNow.AddHours(5).ToString("dd MMM yyyy 'at' hh:mm tt")} (PKT)</td>
    </tr>
</table>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 8px;"">If you made this change, no further action is required.</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 8px;"">If you did <strong>not</strong> request this change, please secure your account immediately by resetting your password and contacting our support team.</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:20px 0 0;"">Regards,<br><strong>BaazWix Security Team</strong></p>";

            return Wrap("Password Reset Confirmation", body, logoUrl, siteUrl);
        }

        public static string Welcome(string userName, string logoUrl, string siteUrl)
        {
            var body = $@"<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">Welcome to <strong>{siteUrl}</strong>, {userName}!</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">Your account has been created successfully. You can now browse thousands of products, add items to your wishlist, and enjoy a smooth shopping experience.</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">If you have any questions, our support team is always happy to help.</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:20px 0 0;"">Happy shopping!<br><strong>BaazWix Team</strong></p>";

            return Wrap("Welcome to BaazWix", body, logoUrl, siteUrl);
        }

        public static string OrderConfirmation(string userName, string orderId, string total, string paymentMethod, string logoUrl, string siteUrl)
        {
            var body = $@"<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">Dear {userName},</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">Thank you for shopping with {siteUrl}! Your order has been placed successfully.</p>
<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""background:#f8fafc; border:1px solid #e5e7eb; border-radius:8px; padding:14px 18px; margin:0 0 18px; width:100%;"">
    <tr>
        <td style=""font-size:13px; color:#374151; padding:4px 0;""><strong>Order Number:</strong></td>
        <td style=""font-size:13px; color:#111827; padding:4px 0; text-align:right;"">#{orderId}</td>
    </tr>
    <tr>
        <td style=""font-size:13px; color:#374151; padding:4px 0;""><strong>Total Amount:</strong></td>
        <td style=""font-size:13px; color:#111827; padding:4px 0; text-align:right;"">PKR {total}</td>
    </tr>
    <tr>
        <td style=""font-size:13px; color:#374151; padding:4px 0;""><strong>Payment Method:</strong></td>
        <td style=""font-size:13px; color:#111827; padding:4px 0; text-align:right;"">{paymentMethod}</td>
    </tr>
</table>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 8px;"">You can track your order status anytime from your account dashboard.</p>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:20px 0 0;"">Thank you for trusting us!<br><strong>BaazWix Team</strong></p>";

            return Wrap("Order Confirmation #" + orderId, body, logoUrl, siteUrl);
        }

        public static string NewRegistration(string userName, string userEmail, string logoUrl, string siteUrl)
        {
            var body = $@"<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">A new user just registered on BaazWix.</p>
<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""background:#f8fafc; border:1px solid #e5e7eb; border-radius:8px; padding:14px 18px; margin:0 0 18px; width:100%;"">
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
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:20px 0 0;"">Regards,<br><strong>BaazWix System</strong></p>";

            return Wrap("New User Registered", body, logoUrl, siteUrl);
        }

        public static string NewOrder(string userName, string orderId, string total, string paymentMethod, string logoUrl, string siteUrl)
        {
            var body = $@"<p style=""font-size:14px; line-height:1.7; color:#374151; margin:0 0 16px;"">A new order has been placed on BaazWix.</p>
<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""background:#f8fafc; border:1px solid #e5e7eb; border-radius:8px; padding:14px 18px; margin:0 0 18px; width:100%;"">
    <tr>
        <td style=""font-size:13px; color:#374151; padding:4px 0;""><strong>Customer:</strong></td>
        <td style=""font-size:13px; color:#111827; padding:4px 0; text-align:right;"">{userName}</td>
    </tr>
    <tr>
        <td style=""font-size:13px; color:#374151; padding:4px 0;""><strong>Order Number:</strong></td>
        <td style=""font-size:13px; color:#111827; padding:4px 0; text-align:right;"">#{orderId}</td>
    </tr>
    <tr>
        <td style=""font-size:13px; color:#374151; padding:4px 0;""><strong>Total Amount:</strong></td>
        <td style=""font-size:13px; color:#111827; padding:4px 0; text-align:right;"">PKR {total}</td>
    </tr>
    <tr>
        <td style=""font-size:13px; color:#374151; padding:4px 0;""><strong>Payment Method:</strong></td>
        <td style=""font-size:13px; color:#111827; padding:4px 0; text-align:right;"">{paymentMethod}</td>
    </tr>
</table>
<p style=""font-size:14px; line-height:1.7; color:#374151; margin:20px 0 0;"">Regards,<br><strong>BaazWix System</strong></p>";

            return Wrap("New Order #" + orderId, body, logoUrl, siteUrl);
        }
    }
}
