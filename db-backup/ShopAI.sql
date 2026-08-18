-- ShopAI Database Export
-- Generated: 2026-08-16 12:09:29
USE [ShopAI];
GO

IF OBJECT_ID('dbo.[BrowseHistories]', 'U') IS NOT NULL DROP TABLE dbo.[BrowseHistories];
CREATE TABLE dbo.[BrowseHistories] (
    [Id] INT NOT NULL,
    [UserId] INT NOT NULL,
    [ProductId] INT NOT NULL,
    [ViewedAt] DATETIME2(7) NOT NULL
);
GO

INSERT INTO dbo.[BrowseHistories] ([Id], [UserId], [ProductId], [ViewedAt]) VALUES
    (N'1', N'1', N'10', N'2026-06-02 20:14:40.4915487'),
    (N'2', N'1', N'10', N'2026-06-02 20:14:39.6291583'),
    (N'3', N'1', N'10', N'2026-06-02 20:14:39.6362338'),
    (N'4', N'2', N'1', N'2026-06-02 20:16:43.3540180'),
    (N'5', N'2', N'3', N'2026-06-02 20:36:07.7462109'),
    (N'6', N'1', N'4', N'2026-06-04 11:12:31.8457039'),
    (N'7', N'2', N'7', N'2026-06-04 11:14:14.7302886'),
    (N'8', N'2', N'6', N'2026-06-04 11:14:29.8384048'),
    (N'9', N'2', N'3', N'2026-06-04 11:15:30.0434525'),
    (N'10', N'2', N'8', N'2026-06-04 11:17:03.2822597'),
    (N'11', N'2', N'2', N'2026-08-15 05:18:25.1784436'),
    (N'12', N'2', N'6', N'2026-08-15 12:21:04.5952856'),
    (N'13', N'2', N'7', N'2026-08-15 13:36:33.7913211'),
    (N'14', N'2', N'1', N'2026-08-15 13:39:08.3069370'),
    (N'15', N'2', N'5', N'2026-08-15 15:13:49.4963538'),
    (N'16', N'2', N'4', N'2026-08-15 15:25:16.7063780'),
    (N'17', N'3', N'4', N'2026-08-15 16:24:40.7853760'),
    (N'18', N'2', N'4', N'2026-08-15 19:19:27.4481150'),
    (N'19', N'2', N'8', N'2026-08-15 19:29:20.1252854'),
    (N'20', N'2', N'7', N'2026-08-15 19:29:21.6590044'),
    (N'21', N'2', N'1', N'2026-08-16 06:23:27.7616236'),
    (N'22', N'3', N'4', N'2026-08-16 06:31:16.0557693'),
    (NULL),
;
GO

IF OBJECT_ID('dbo.[CartItems]', 'U') IS NOT NULL DROP TABLE dbo.[CartItems];
CREATE TABLE dbo.[CartItems] (
    [Id] INT NOT NULL,
    [UserId] INT NOT NULL,
    [ProductId] INT NOT NULL,
    [Quantity] INT NOT NULL
);
GO

INSERT INTO dbo.[CartItems] ([Id], [UserId], [ProductId], [Quantity]) VALUES
    (N'14', N'2', N'4', N'1'),
    (N'15', N'2', N'7', N'1'),
    (N'16', N'2', N'1', N'1'),
    (NULL),
;
GO

IF OBJECT_ID('dbo.[ChatbotLogs]', 'U') IS NOT NULL DROP TABLE dbo.[ChatbotLogs];
CREATE TABLE dbo.[ChatbotLogs] (
    [Id] INT NOT NULL,
    [UserId] INT NULL,
    [Query] NVARCHAR(MAX) NOT NULL,
    [Response] NVARCHAR(MAX) NULL,
    [Timestamp] DATETIME2(7) NOT NULL
);
GO

INSERT INTO dbo.[ChatbotLogs] ([Id], [UserId], [Query], [Response], [Timestamp]) VALUES
    (N'1', N'1', N'HAVE LAPTOPS', N'Here are our 3 premium high-end products:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'• **Smartphone X15 Pro** (Phones) – PKR 104,499', N'2026-06-04 11:12:12.5342889'),
    (N'2', N'1', N'hi', N'Hey there! ?? How can I help you today?', N'2026-07-18 08:19:26.1852483'),
    (N'3', N'1', N'product details', N'Sorry, we don''t have that product available in our store right now. ?? Can I help you find something else?', N'2026-07-18 08:19:39.7715071'),
    (N'4', NULL, N'hi', N'Hey there! ?? How can I help you today?', N'2026-08-15 05:08:23.3977483'),
    (N'5', NULL, N'computer', N'Sorry, we don''t have that product available in our store right now. ?? Can I help you find something else?', N'2026-08-15 05:08:30.1682232'),
    (N'6', NULL, N'laptop', N'Here are our 3 premium high-end products:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'• **Smartphone X15 Pro** (Phones) – PKR 104,499', N'2026-08-15 05:08:39.9563239'),
    (N'7', NULL, N'Hello', N'Hey there! ?? How can I help you today?', N'2026-08-15 11:45:42.2862859'),
    (N'8', NULL, N'Show me products', N'Sorry, we don''t have that product available in our store right now. ?? Can I help you find something else?', N'2026-08-15 11:45:55.3124954'),
    (N'9', NULL, N'help', N'I can show you products, prices, compare items, and give recommendations — but only from our store catalog. How can I assist you?', N'2026-08-15 11:46:13.2106950'),
    (N'10', NULL, N'budget', N'Here are our 3 most budget-friendly options:'),
    (NULL),
    (N'• **USB-C Hub 7-in-1** (Accessories) – PKR 4,999'),
    (N'• **Wireless Gaming Mouse** (Accessories) – PKR 9,999'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 12,999', N'2026-08-15 11:46:37.0682847'),
    (N'11', NULL, N'budget', N'Here are our 3 most budget-friendly options:'),
    (NULL),
    (N'• **USB-C Hub 7-in-1** (Accessories) – PKR 4,999'),
    (N'• **Wireless Gaming Mouse** (Accessories) – PKR 9,999'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 12,999', N'2026-08-15 11:49:00.2044685'),
    (N'12', NULL, N'hi', N'Hey there! ?? How can I help you today?', N'2026-08-15 11:51:13.3538491'),
    (N'13', NULL, N'give me products', N'Sorry, we don''t have that product available in our store right now. ?? Can I help you find something else?', N'2026-08-15 11:51:27.3023491'),
    (N'14', NULL, N'Show trending products', N'Sorry, we don''t have that product available in our store right now. ?? Can I help you find something else?', N'2026-08-15 11:51:35.5152479'),
    (N'15', NULL, N'Show new arrivals', N'Sorry, we don''t have that product available in our store right now. ?? Can I help you find something else?', N'2026-08-15 11:51:41.8001157'),
    (N'16', NULL, N'Show sale items', N'Sorry, we don''t have that product available in our store right now. ?? Can I help you find something else?', N'2026-08-15 11:51:43.6064382'),
    (N'17', NULL, N'Show trending products', N'Sorry, we don''t have that product available in our store right now. ?? Can I help you find something else?', N'2026-08-15 11:51:45.0056730'),
    (N'18', NULL, N'laptops', N'Here are our 3 premium high-end products:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'• **Smartphone X15 Pro** (Phones) – PKR 104,499', N'2026-08-15 11:52:02.5523347'),
    (N'19', NULL, N'how much product you have', N'Sorry, we don''t have that product available in our store right now. ?? Can I help you find something else?', N'2026-08-15 11:52:16.3614671'),
    (N'20', NULL, N'laptop', N'Here are our 3 premium high-end products:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'• **Smartphone X15 Pro** (Phones) – PKR 104,499', N'2026-08-15 11:53:46.4557158'),
    (N'21', NULL, N'headphones', N'Here are the details from our catalog:'),
    (NULL),
    (N'**Wireless Noise-Cancelling Headphones**'),
    (N'Category: Audio'),
    (N'Price: PKR 25,499'),
    (N'_Premium sound, 30hr battery, ANC technology._', N'2026-08-15 11:54:03.4828261')
;
GO
INSERT INTO dbo.[ChatbotLogs] ([Id], [UserId], [Query], [Response], [Timestamp]) VALUES
    (N'22', NULL, N'how many products', N'Sorry, we don''t have that product available in our store right now. ?? Can I help you find something else?', N'2026-08-15 11:54:15.3979497'),
    (N'23', NULL, N'show all', N'Here is our currently available catalog:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'• **Wireless Noise-Cancelling Headphones** (Audio) – PKR 25,499'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 1', N'2026-08-15 11:54:48.2099794'),
    (N'24', NULL, N'how much product you have', N'Sorry, we don''t have that product available in our store right now. ?? Can I help you find something else?', N'2026-08-15 11:57:17.1125991'),
    (N'25', NULL, N'show all products', N'Here is our currently available catalog:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'• **Wireless Noise-Cancelling Headphones** (Audio) – PKR 25,499'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 1', N'2026-08-15 11:58:10.9733533'),
    (N'26', NULL, N'low price product', N'Sorry, we don''t have that product available in our store right now. ?? Can I help you find something else?', N'2026-08-15 11:58:32.9242225'),
    (N'27', NULL, N'show all', N'Here is our currently available catalog:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'• **Wireless Noise-Cancelling Headphones** (Audio) – PKR 25,499'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 1', N'2026-08-15 12:01:27.6458842'),
    (N'28', NULL, N'show lowest price products', N'Here are our 3 most budget-friendly options:'),
    (NULL),
    (N'• **USB-C Hub 7-in-1** (Accessories) – PKR 4,999'),
    (N'• **Wireless Gaming Mouse** (Accessories) – PKR 9,999'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 12,999', N'2026-08-15 12:03:04.6717554'),
    (N'29', NULL, N'wireless gaming mouse discription', N'Here are the details from our catalog:'),
    (NULL),
    (N'**Wireless Noise-Cancelling Headphones**'),
    (N'Category: Audio'),
    (N'Price: PKR 25,499'),
    (N'_Premium sound, 30hr battery, ANC technology._'),
    (NULL),
    (N'**Wireless Gaming Mouse**'),
    (N'Category: Accessories'),
    (N'Price: PKR 9,999'),
    (N'_25,000 DPI, 70hr battery, u', N'2026-08-15 12:03:58.1717906'),
    (N'30', NULL, N'Wireless Gaming Mouse only this..give me link of this', N'Hey there! ?? How can I help you today?', N'2026-08-15 12:04:32.3397950'),
    (N'31', NULL, N'Wireless Gaming Mouse only this..give me link of this', N'Hey there! ?? How can I help you today?', N'2026-08-15 12:04:42.4802839'),
    (N'32', NULL, N'wireless gaming mouse', N'Here are the details from our catalog:'),
    (NULL),
    (N'**Wireless Noise-Cancelling Headphones**'),
    (N'Category: Audio'),
    (N'Price: PKR 25,499'),
    (N'_Premium sound, 30hr battery, ANC technology._'),
    (NULL),
    (N'**Wireless Gaming Mouse**'),
    (N'Category: Accessories'),
    (N'Price: PKR 9,999'),
    (N'_25,000 DPI, 70hr battery, u', N'2026-08-15 12:04:56.7139557')
;
GO
INSERT INTO dbo.[ChatbotLogs] ([Id], [UserId], [Query], [Response], [Timestamp]) VALUES
    (N'33', NULL, N'wireless gaming mouse', N'**Wireless Noise-Cancelling Headphones**'),
    (N'Category: Audio'),
    (N'Price: PKR 25,499'),
    (N'_Description: Premium sound, 30hr battery, ANC technology._'),
    (N'[View Product ?](/Products/Detail/3)', N'2026-08-15 12:13:20.5376580'),
    (N'34', NULL, N'budget', N'Here are our 3 most budget-friendly options:'),
    (NULL),
    (N'• **USB-C Hub 7-in-1** (Accessories) – PKR 4,999'),
    (N'• **Wireless Gaming Mouse** (Accessories) – PKR 9,999'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 12,999', N'2026-08-15 12:13:21.5483277'),
    (N'35', NULL, N'give me link of gaming laptop', N'[View Gaming Laptop Pro](/Products/Detail/1) — PKR 170,999', N'2026-08-15 12:13:21.7321135'),
    (N'36', NULL, N'wireless gaming mouse', N'**Wireless Gaming Mouse**'),
    (N'Category: Accessories'),
    (N'Price: PKR 9,999'),
    (N'_Description: 25,000 DPI, 70hr battery, ultra-lightweight._'),
    (N'[View Product ?](/Products/Detail/6)', N'2026-08-15 12:14:58.9364727'),
    (N'37', NULL, N'budget', N'[View Gaming Laptop Pro](/Products/Detail/1) — PKR 170,999', N'2026-08-15 12:14:59.6575264'),
    (N'38', NULL, N'give me link of gaming laptop', N'Here are our 3 premium high-end products:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'• **Smartphone X15 Pro** (Phones) – PKR 104,499', N'2026-08-15 12:14:59.8386160'),
    (N'39', NULL, N'budget', N'[View Gaming Laptop Pro](/Products/Detail/1) — PKR 170,999', N'2026-08-15 12:15:08.5025087'),
    (N'40', NULL, N'show all', N'Here is our currently available catalog:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'• **Wireless Noise-Cancelling Headphones** (Audio) – PKR 25,499'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 1', N'2026-08-15 12:15:35.2329402'),
    (N'41', NULL, N'link of gaming laptop', N'[View Gaming Laptop Pro](/Products/Detail/1) — PKR 170,999', N'2026-08-15 12:15:35.3719141'),
    (N'42', NULL, N'wireless gaming mouse', N'**Wireless Gaming Mouse**'),
    (N'Category: Accessories'),
    (N'Price: PKR 9,999'),
    (N'_Description: 25,000 DPI, 70hr battery, ultra-lightweight._'),
    (N'[View Product ?](/Products/Detail/6)', N'2026-08-15 12:17:45.7622388'),
    (N'43', NULL, N'budget', N'Here are our 3 most budget-friendly options:'),
    (NULL),
    (N'• **USB-C Hub 7-in-1** (Accessories) – PKR 4,999'),
    (N'• **Wireless Gaming Mouse** (Accessories) – PKR 9,999'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 12,999', N'2026-08-15 12:17:46.5144158'),
    (N'44', NULL, N'link of gaming laptop', N'[View Gaming Laptop Pro](/Products/Detail/1) — PKR 170,999', N'2026-08-15 12:17:46.6475840'),
    (N'45', NULL, N'show all', N'Here is our currently available catalog:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'• **Wireless Noise-Cancelling Headphones** (Audio) – PKR 25,499'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 1', N'2026-08-15 12:17:46.8298117'),
    (N'46', NULL, N'price of headphones', N'**Wireless Noise-Cancelling Headphones**'),
    (N'Category: Audio'),
    (N'Price: PKR 25,499')
;
GO
INSERT INTO dbo.[ChatbotLogs] ([Id], [UserId], [Query], [Response], [Timestamp]) VALUES
    (N'_Description: Premium sound, 30hr battery, ANC technology._'),
    (N'[View Product ?](/Products/Detail/3)', N'2026-08-15 12:17:46.9429252'),
    (N'47', NULL, N'xyz123random', N'I couldn''t find that in our catalog. ?? Can I help you find something else?', N'2026-08-15 12:17:47.0640086'),
    (N'48', NULL, N'wireless mouse', N'**Wireless Gaming Mouse**'),
    (N'Category: Accessories'),
    (N'Price: PKR 9,999'),
    (N'_Description: 25,000 DPI, 70hr battery, ultra-lightweight._'),
    (N'[View Product ?](/Products/Detail/6)', N'2026-08-15 12:20:37.9659496'),
    (N'49', NULL, N'show me project', N'I couldn''t find that in our catalog. ?? Can I help you find something else?', N'2026-08-15 13:08:11.5651348'),
    (N'50', NULL, N'price', N'**Gaming Laptop Pro**'),
    (N'Category: Laptops'),
    (N'Price: PKR 170,999'),
    (N'_Description: High-performance gaming laptop with RTX 4070, 16GB RAM, 512GB SSD._'),
    (N'[View Product ?](/Products/Detail/1)', N'2026-08-15 13:08:20.2446818'),
    (N'51', NULL, N'budget', N'Here are our 3 most budget-friendly options:'),
    (NULL),
    (N'• **USB-C Hub 7-in-1** (Accessories) – PKR 4,999'),
    (N'• **Wireless Gaming Mouse** (Accessories) – PKR 9,999'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 12,999', N'2026-08-15 13:08:26.2905831'),
    (N'52', NULL, N'show me product', N'I couldn''t find that in our catalog. ?? Can I help you find something else?', N'2026-08-15 13:08:40.4549783'),
    (N'53', NULL, N'show me all products', N'Here is our currently available catalog:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'• **Wireless Noise-Cancelling Headphones** (Audio) – PKR 25,499'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 1', N'2026-08-15 13:08:52.3468270'),
    (N'54', N'2', N'give me products details', N'I couldn''t find that in our catalog. ?? Can I help you find something else?', N'2026-08-15 13:38:18.3827841'),
    (N'55', N'2', N'show me all produts name', N'I couldn''t find that in our catalog. ?? Can I help you find something else?', N'2026-08-15 13:38:49.8081608'),
    (N'56', N'2', N';', N'**Gaming Laptop Pro**'),
    (N'Category: Laptops'),
    (N'Price: PKR 170,999'),
    (N'_Description: High-performance gaming laptop with RTX 4070, 16GB RAM, 512GB SSD._'),
    (N'[View Product ?](/Products/Detail/1)', N'2026-08-15 13:38:55.6564836'),
    (N'57', N'2', N'show me all products details', N'Here is our currently available catalog:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'• **Wireless Noise-Cancelling Headphones** (Audio) – PKR 25,499'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 1', N'2026-08-15 13:39:28.8857904'),
    (N'58', NULL, N'hi', N'Hey there! ?? How can I help you today?', N'2026-08-15 14:47:36.3044205'),
    (N'59', NULL, N'show me all products detail', N'Here is our currently available catalog:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'• **Wireless Noise-Cancelling Headphones** (Audio) – PKR 25,499'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 1', N'2026-08-15 14:48:01.1967087'),
    (N'60', NULL, N'how i can contact', N'I couldn''t find that in our catalog. ?? Can I help you find something else?', N'2026-08-15 14:48:20.6843635'),
    (N'61', NULL, N'how i can contact in admin', N'I couldn''t find that in our catalog. ?? Can I help you find something else?', N'2026-08-15 14:48:47.0812616'),
    (N'62', NULL, N'how i can contact with admin', N'I couldn''t find that in our catalog. ?? Can I help you find something else?', N'2026-08-15 14:49:07.3272956'),
    (N'63', NULL, N'show product details', N'I couldn''t find that in our catalog. ?? Can I help you find something else?', N'2026-08-15 14:49:34.3145482')
;
GO
INSERT INTO dbo.[ChatbotLogs] ([Id], [UserId], [Query], [Response], [Timestamp]) VALUES
    (N'64', NULL, N'lowest price rate', N'Here are our 3 most budget-friendly options:'),
    (NULL),
    (N'• **USB-C Hub 7-in-1** (Accessories) – PKR 4,999'),
    (N'• **Wireless Gaming Mouse** (Accessories) – PKR 9,999'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 12,999', N'2026-08-15 14:49:57.7569075'),
    (N'65', NULL, N'laptops', N'Here are our 3 premium high-end products:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'• **Smartphone X15 Pro** (Phones) – PKR 104,499', N'2026-08-15 14:50:05.1804452'),
    (N'66', NULL, N'budget', N'Here are our 3 most budget-friendly options:'),
    (NULL),
    (N'• **USB-C Hub 7-in-1** (Accessories) – PKR 4,999'),
    (N'• **Wireless Gaming Mouse** (Accessories) – PKR 9,999'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 12,999', N'2026-08-15 14:50:15.2848809'),
    (N'67', NULL, N'expensive', N'Here are our 3 premium high-end products:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'• **Smartphone X15 Pro** (Phones) – PKR 104,499', N'2026-08-15 14:50:22.9560079'),
    (N'68', NULL, N'show all', N'Here is our currently available catalog:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'• **Wireless Noise-Cancelling Headphones** (Audio) – PKR 25,499'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 1', N'2026-08-15 14:50:28.0557843'),
    (N'69', NULL, N'new', N'I couldn''t find that in our catalog. ?? Can I help you find something else?', N'2026-08-15 14:50:35.2369527'),
    (N'70', NULL, N'how i contact', N'I couldn''t find that in our catalog. ?? Can I help you find something else?', N'2026-08-15 14:51:02.7239517'),
    (N'71', NULL, N'how i can contact', N'You can reach our team at:'),
    (N'?? qamarbaloch2023@gmail.com'),
    (N'?? +92-300-1234567'),
    (NULL),
    (N'Or visit our Contact page: /Home/Contact', N'2026-08-15 14:56:23.8890591'),
    (N'72', NULL, N'new', N'Here are our latest arrivals:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'• **Wireless Noise-Cancelling Headphones** (Audio) – PKR 25,499'),
    (N'• **Mechanical Gaming Keyboard** (Accessories) – PKR 12,999'),
    (N'• **4', N'2026-08-15 14:56:25.6873763'),
    (N'73', NULL, N'show product details', N'Please tell me the specific product name you''d like details for, or type ''show all'' to see our full catalog! ???', N'2026-08-15 14:56:25.9122452'),
    (N'74', NULL, N'how i can contact with admin', N'You can reach our team at:'),
    (N'?? qamarbaloch2023@gmail.com'),
    (N'?? +92-300-1234567'),
    (NULL),
    (N'Or visit our Contact page: /Home/Contact', N'2026-08-15 14:56:56.3505180'),
    (N'75', NULL, N'how i contact', N'You can reach our team at:'),
    (N'?? qamarbaloch2023@gmail.com'),
    (N'?? +92-300-1234567'),
    (NULL)
;
GO
INSERT INTO dbo.[ChatbotLogs] ([Id], [UserId], [Query], [Response], [Timestamp]) VALUES
    (N'Or visit our Contact page: /Home/Contact', N'2026-08-15 14:56:56.5834116'),
    (N'76', NULL, N'show all', N'Here is our currently available catalog:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'[View Product ?](/Products/Detail/1)'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'[View Product ?](/Products/Detail/2)'),
    (N'• **Wireless Noise-Cancelling Headphones', N'2026-08-15 15:00:35.1179407'),
    (N'77', NULL, N'budget', N'Here are our 3 most budget-friendly options:'),
    (NULL),
    (N'• **USB-C Hub 7-in-1** (Accessories) – PKR 4,999'),
    (N'[View Product ?](/Products/Detail/9)'),
    (N'• **Wireless Gaming Mouse** (Accessories) – PKR 9,999'),
    (N'[View Product ?](/Products/Detail/6)'),
    (N'• **Mechanical Gaming Keyboar', N'2026-08-15 15:00:35.8361199'),
    (N'78', N'2', N'LOWEST PRICE OF PRODUCTS', N'Here are our 3 most budget-friendly options:'),
    (NULL),
    (N'• **USB-C Hub 7-in-1** (Accessories) – PKR 4,999'),
    (N'[View Product ?](/Products/Detail/9)'),
    (N'• **Wireless Gaming Mouse** (Accessories) – PKR 9,999'),
    (N'[View Product ?](/Products/Detail/6)'),
    (N'• **Mechanical Gaming Keyboar', N'2026-08-15 16:04:49.3121605'),
    (N'79', N'2', N'OK', N'Got it! Let me know if you need help finding any products from our catalog. ??', N'2026-08-15 16:05:00.2345208'),
    (N'80', N'2', N'show all', N'Here is our currently available catalog:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'[View Product ?](/Products/Detail/1)'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'[View Product ?](/Products/Detail/2)'),
    (N'• **Wireless Noise-Cancelling Headphones', N'2026-08-15 16:05:06.1156306'),
    (N'81', N'2', N'expensive', N'Here are our 3 premium high-end products:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'[View Product ?](/Products/Detail/1)'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'[View Product ?](/Products/Detail/2)'),
    (N'• **Smartphone X15 Pro** (Phones) – PKR', N'2026-08-15 16:05:13.9029764'),
    (N'82', N'2', N'budget', N'Here are our 3 most budget-friendly options:'),
    (NULL),
    (N'• **USB-C Hub 7-in-1** (Accessories) – PKR 4,999'),
    (N'[View Product ?](/Products/Detail/9)'),
    (N'• **Wireless Gaming Mouse** (Accessories) – PKR 9,999'),
    (N'[View Product ?](/Products/Detail/6)'),
    (N'• **Mechanical Gaming Keyboar', N'2026-08-15 16:05:15.2803342'),
    (N'83', NULL, N'j', N'**Gaming Laptop Pro**'),
    (N'Category: Laptops'),
    (N'Price: PKR 170,999'),
    (N'_Description: High-performance gaming laptop with RTX 4070, 16GB RAM, 512GB SSD._'),
    (N'[View Product ?](/Products/Detail/1)', N'2026-08-15 19:03:14.0942233'),
    (N'84', NULL, N'budget', N'Here are our 3 most budget-friendly options:')
;
GO
INSERT INTO dbo.[ChatbotLogs] ([Id], [UserId], [Query], [Response], [Timestamp]) VALUES
    (N'• **USB-C Hub 7-in-1** (Accessories) – PKR 4,999'),
    (N'[View Product ?](/Products/Detail/9)'),
    (N'• **Wireless Gaming Mouse** (Accessories) – PKR 9,999'),
    (N'[View Product ?](/Products/Detail/6)'),
    (N'• **Mechanical Gaming Keyboar', N'2026-08-15 19:03:26.2869148'),
    (N'85', NULL, N'give me all products', N'Here is our currently available catalog:'),
    (NULL),
    (N'• **Gaming Laptop Pro** (Laptops) – PKR 170,999'),
    (N'[View Product ?](/Products/Detail/1)'),
    (N'• **UltraBook Slim 14** (Laptops) – PKR 142,499'),
    (N'[View Product ?](/Products/Detail/2)'),
    (N'• **Wireless Noise-Cancelling Headphones', N'2026-08-16 06:22:32.9134117'),
    (NULL),
;
GO

IF OBJECT_ID('dbo.[ContactMessages]', 'U') IS NOT NULL DROP TABLE dbo.[ContactMessages];
CREATE TABLE dbo.[ContactMessages] (
    [Id] INT NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(MAX) NOT NULL,
    [Message] NVARCHAR(MAX) NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL,
    [IsRead] BIT NOT NULL
);
GO


IF OBJECT_ID('dbo.[HeroSlides]', 'U') IS NOT NULL DROP TABLE dbo.[HeroSlides];
CREATE TABLE dbo.[HeroSlides] (
    [Id] INT NOT NULL,
    [Tagline] NVARCHAR(MAX) NOT NULL,
    [Title] NVARCHAR(MAX) NOT NULL,
    [CategoryLabel] NVARCHAR(MAX) NOT NULL,
    [Hashtag] NVARCHAR(MAX) NULL,
    [LinkUrl] NVARCHAR(MAX) NULL,
    [ImagePath] NVARCHAR(MAX) NULL,
    [SortOrder] INT NOT NULL,
    [IsActive] BIT NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL,
    [DisplayPrice] DECIMAL(18,2) NULL,
    [ProductId] INT NULL
);
GO

INSERT INTO dbo.[HeroSlides] ([Id], [Tagline], [Title], [CategoryLabel], [Hashtag], [LinkUrl], [ImagePath], [SortOrder], [IsActive], [CreatedAt], [DisplayPrice], [ProductId]) VALUES
    (N'1', N'qww', N'12', N'qqq', N'1212', N'qqq', N'/images/hero_dfd2b168-60cd-4ce9-bbcf-6a74bfd8f686.jpg', N'1', N'1', N'2026-06-02 20:13:55.9016556', N'12999.00', N'4'),
    (N'3', N'qww', N'Laptop', N'qqq', N'1212', N'qqq', N'/images/hero_c75d04b1-13a4-455a-8509-ba3852c51f45.jpg', N'236', N'1', N'2026-06-04 11:07:42.6369455', N'104499.05', NULL),
    (N'4', N'qww', N'ere', N'qqq', N'1212', N'qqq', N'/images/hero_274944d8-ebca-4426-80f3-42d030b774a5.jpg', N'0', N'1', N'2026-06-04 11:08:21.1943390', N'104499.05', N'4'),
    (N'5', N'qww', N'Heaadphone', N'qqq', N'1212', N'qqq', N'/images/hero_22e07be7-ca9e-4bf8-8bdc-ed8914e9664d.jpg', N'2', N'1', N'2026-06-04 11:09:01.7380403', N'104499.05', N'9'),
    (NULL),
;
GO

IF OBJECT_ID('dbo.[OrderItems]', 'U') IS NOT NULL DROP TABLE dbo.[OrderItems];
CREATE TABLE dbo.[OrderItems] (
    [Id] INT NOT NULL,
    [OrderId] INT NOT NULL,
    [ProductId] INT NOT NULL,
    [Quantity] INT NOT NULL,
    [UnitPrice] DECIMAL(18,2) NOT NULL
);
GO

INSERT INTO dbo.[OrderItems] ([Id], [OrderId], [ProductId], [Quantity], [UnitPrice]) VALUES
    (N'1', N'1', N'3', N'1', N'25499.15'),
    (N'2', N'2', N'3', N'1', N'25499.15'),
    (N'3', N'3', N'8', N'1', N'11999.20'),
    (N'4', N'4', N'9', N'1', N'4999.00'),
    (N'5', N'5', N'6', N'1', N'9999.00'),
    (N'6', N'6', N'5', N'2', N'68999.08'),
    (N'7', N'6', N'4', N'1', N'12999.00'),
    (NULL),
;
GO

IF OBJECT_ID('dbo.[Orders]', 'U') IS NOT NULL DROP TABLE dbo.[Orders];
CREATE TABLE dbo.[Orders] (
    [Id] INT NOT NULL,
    [UserId] INT NOT NULL,
    [TotalAmount] DECIMAL(18,2) NOT NULL,
    [DeliveryAddress] NVARCHAR(MAX) NULL,
    [PaymentMethod] NVARCHAR(MAX) NULL,
    [Status] NVARCHAR(MAX) NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL
);
GO

INSERT INTO dbo.[Orders] ([Id], [UserId], [TotalAmount], [DeliveryAddress], [PaymentMethod], [Status], [CreatedAt]) VALUES
    (N'1', N'2', N'25499.15', N'Qamar Zaman'),
    (N'hk'),
    (N'jkjk, 89i'),
    (N'Pakistan'),
    (N'Phone: 9230489025', N'Cash on Delivery', N'delivered', N'2026-06-03 10:03:10.9793886'),
    (N'2', N'2', N'25499.15', N'Qamar Zaman'),
    (N'131'),
    (N'mmmk, 132'),
    (N'Pakistan'),
    (N'Phone: 9230489025', N'Cash on Delivery', N'pending', N'2026-06-04 11:16:24.5997362'),
    (N'3', N'2', N'11999.20', N'Qamar Zaman'),
    (N'131'),
    (N'mmmk, 132'),
    (N'Pakistan'),
    (N'Phone: 9230489025', N'Cash on Delivery', N'pending', N'2026-06-04 11:17:32.2738230'),
    (N'4', N'2', N'4999.00', N'Qamar Zaman'),
    (N'akknsdkls'),
    (N'?????, 03727'),
    (N'Pakistan'),
    (N'Phone: 03216068091', N'Cash on Delivery', N'pending', N'2026-07-18 08:12:50.9526590'),
    (N'5', N'2', N'9999.00', N'Qamar Zaman'),
    (N'jhanian tcs'),
    (N'jahanina, 6767'),
    (N'Pakistan'),
    (N'Phone: 03216068091', N'Cash on Delivery', N'delivered', N'2026-08-15 12:21:56.2123652'),
    (N'6', N'2', N'150997.16', N'Qamar Zaman'),
    (N'Mulatn tehsil chowk'),
    (N'MulTAN, I0O'),
    (N'Pakistan'),
    (N'Phone: 03216068091', N'Cash on Delivery', N'confirmed', N'2026-08-15 16:04:05.3546427'),
    (NULL),
;
GO

IF OBJECT_ID('dbo.[Products]', 'U') IS NOT NULL DROP TABLE dbo.[Products];
CREATE TABLE dbo.[Products] (
    [Id] INT NOT NULL,
    [Name] NVARCHAR(MAX) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Price] DECIMAL(18,2) NOT NULL,
    [Discount] DECIMAL(18,2) NOT NULL,
    [Stock] INT NOT NULL,
    [Category] NVARCHAR(MAX) NULL,
    [Brand] NVARCHAR(MAX) NULL,
    [ImagePath] NVARCHAR(MAX) NULL,
    [CreatedAt] DATETIME2(7) NOT NULL
);
GO

INSERT INTO dbo.[Products] ([Id], [Name], [Description], [Price], [Discount], [Stock], [Category], [Brand], [ImagePath], [CreatedAt]) VALUES
    (N'1', N'Gaming Laptop Pro', N'High-performance gaming laptop with RTX 4070, 16GB RAM, 512GB SSD.', N'189999.00', N'10.00', N'15', N'Laptops', N'Asus', N'/images/b3aad09e-fff1-4ccd-afe9-6233e6c4d4ec.webp', N'2024-01-01 00:00:00.0000000'),
    (N'2', N'UltraBook Slim 14', N'Lightweight business laptop, Intel Core i7, 16GB RAM, 1TB SSD.', N'149999.00', N'5.00', N'20', N'Laptops', N'Dell', N'/images/1202c2be-d48f-417b-bca4-8e793521a273.webp', N'2024-01-01 00:00:00.0000000'),
    (N'3', N'Wireless Noise-Cancelling Headphones', N'Premium sound, 30hr battery, ANC technology.', N'29999.00', N'15.00', N'48', N'Audio', N'Sony', N'/images/ed1c1a69-4889-42a8-a372-75a5f0cef378.webp', N'2024-01-01 00:00:00.0000000'),
    (N'4', N'Mechanical Gaming Keyboard', N'RGB backlit, Cherry MX switches, USB-C.', N'12999.00', N'.00', N'34', N'Accessories', N'Logitech', N'/images/d8ce1649-2ec9-4254-b910-ef7215fe5c35.jpg', N'2024-01-01 00:00:00.0000000'),
    (N'5', N'4K Curved Monitor 27"', N'144Hz refresh rate, 1ms response, HDR400.', N'74999.00', N'8.00', N'8', N'Monitors', N'Samsung', N'/images/48d48cea-18b9-4bac-a090-3b50df093dd8.jpeg', N'2024-01-01 00:00:00.0000000'),
    (N'6', N'Wireless Gaming Mouse', N'25,000 DPI, 70hr battery, ultra-lightweight.', N'9999.00', N'.00', N'59', N'Accessories', N'Razer', N'/images/51566b92-7398-453c-a029-7707e0a48907.jpg', N'2024-01-01 00:00:00.0000000'),
    (N'7', N'Smartphone X15 Pro', N'6.7" AMOLED, 200MP camera, 5000mAh, Snapdragon 8 Gen 3.', N'109999.00', N'5.00', N'25', N'Phones', N'Samsung', N'/images/349af333-3861-45fa-9479-c9d0f8b2c6ae.png', N'2024-01-01 00:00:00.0000000'),
    (N'8', N'True Wireless Earbuds', N'ANC, 36hr total battery, IPX5 waterproof.', N'14999.00', N'20.00', N'79', N'Audio', N'Apple', N'/images/e0115d87-92e8-462a-b210-5b802b055ac7.jpg', N'2024-01-01 00:00:00.0000000'),
    (N'9', N'USB-C Hub 7-in-1', N'HDMI 4K, 100W PD, SD/MicroSD, 3x USB-A.', N'4999.00', N'.00', N'99', N'Accessories', N'Anker', N'/images/0d1208e8-7e96-47f1-930b-43c4ae557ce0.jpg', N'2024-01-01 00:00:00.0000000'),
    (N'10', N'Portable SSD 1TB', N'Read 1050MB/s, USB 3.2 Gen 2, shock-proof.', N'19999.00', N'10.00', N'40', N'Storage', N'Samsung', N'/images/41f6a105-0ee4-4b93-bb67-d5a756119629.jpg', N'2024-01-01 00:00:00.0000000'),
    (NULL),
;
GO

IF OBJECT_ID('dbo.[Reviews]', 'U') IS NOT NULL DROP TABLE dbo.[Reviews];
CREATE TABLE dbo.[Reviews] (
    [Id] INT NOT NULL,
    [ProductId] INT NOT NULL,
    [UserId] INT NOT NULL,
    [Rating] INT NOT NULL,
    [Comment] NVARCHAR(1000) NULL,
    [CreatedAt] DATETIME2(7) NOT NULL
);
GO

INSERT INTO dbo.[Reviews] ([Id], [ProductId], [UserId], [Rating], [Comment], [CreatedAt]) VALUES
    (N'1', N'3', N'2', N'5', N'GOOOD EXPERINCE', N'2026-06-04 11:16:54.3011580'),
    (NULL),
;
GO

IF OBJECT_ID('dbo.[SaleAnalytics]', 'U') IS NOT NULL DROP TABLE dbo.[SaleAnalytics];
CREATE TABLE dbo.[SaleAnalytics] (
    [Id] INT NOT NULL,
    [Period] NVARCHAR(MAX) NOT NULL,
    [Label] NVARCHAR(MAX) NOT NULL,
    [TotalSales] DECIMAL(18,2) NOT NULL,
    [OrderCount] INT NOT NULL,
    [ComputedAt] DATETIME2(7) NOT NULL
);
GO


IF OBJECT_ID('dbo.[Users]', 'U') IS NOT NULL DROP TABLE dbo.[Users];
CREATE TABLE dbo.[Users] (
    [Id] INT NOT NULL,
    [FullName] NVARCHAR(MAX) NOT NULL,
    [Email] NVARCHAR(MAX) NOT NULL,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [Phone] NVARCHAR(MAX) NULL,
    [Role] NVARCHAR(MAX) NOT NULL,
    [IsLocked] BIT NOT NULL,
    [FailedAttempts] INT NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL,
    [Balance] DECIMAL(18,2) NOT NULL
);
GO

INSERT INTO dbo.[Users] ([Id], [FullName], [Email], [PasswordHash], [Phone], [Role], [IsLocked], [FailedAttempts], [CreatedAt], [Balance]) VALUES
    (N'1', N'Admin', N'qamr2026zaman@shop.com', N'$2a$11$er/rWvJvUGL4UdHoN0iTXexco2pMcVaE2mmU8GBI7YPAHt0QnrJqq', N'0300-0000000', N'admin', N'0', N'0', N'2026-03-01 00:00:00.0000000', N'.00'),
    (N'2', N'Qamar Zaman', N'qamarbaloch2023@gmail.com', N'$2a$11$ToenOw0V2xVOqPJIqoCO/uiQzAGfpQznStNlJUBULKMoNK0koUhii', N'03216068091', N'customer', N'0', N'0', N'2026-06-02 20:05:40.2981762', N'1573949.34'),
    (N'3', N'Admin', N'admin@shop.com', N'$2a$11$LX99SXQPAaze0rMQSZTxGuPW0GuKLzFfSWd287Vdcr66oy3xFXLYm', N'0300-0000000', N'admin', N'0', N'0', N'2026-08-15 21:13:08.6900000', N'.00'),
    (NULL),
;
GO

IF OBJECT_ID('dbo.[WishlistItems]', 'U') IS NOT NULL DROP TABLE dbo.[WishlistItems];
CREATE TABLE dbo.[WishlistItems] (
    [Id] INT NOT NULL,
    [UserId] INT NOT NULL,
    [ProductId] INT NOT NULL,
    [AddedAt] DATETIME2(7) NOT NULL
);
GO


