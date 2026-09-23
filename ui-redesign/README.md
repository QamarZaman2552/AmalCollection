# Amal Collection – Ladies Suits UI Redesign (v3 · Bootstrap 5)

Yeh folder ek **standalone UI prototype** hai (**Bootstrap 5 + Bootstrap Icons**). Browser mein directly khol kar dekh sakte hain:

```
ui-redesign/index.html      → Homepage (Hero + Season + Products + How-to-Order)
ui-redesign/product.html    → Product Detail (size/color, guest-friendly)
ui-redesign/about.html      → About Us (story, values, size guide, FAQs)
ui-redesign/checkout.html   → Guest Checkout (bina login ke order)
```

**Stack:** Bootstrap 5.3 (CDN) · Bootstrap Icons 1.11 (CDN) · Google Fonts (Playfair Display + Inter)  
**Icons rule:** Sirf Bootstrap Icons (`bi-*`) — koi emoji ya custom SVG nahi.

---

## v2 Requirements (is round mein kya change hua)

| Requirement | Status |
|---|---|
| **Sirf ladies collection** – summer & winter suits | ✅ Men/Kids/Ethnic categories hata diye – sirf Summer Suits + Winter Suits |
| **About Us page** | ✅ `about.html` – story, values, size guide table, FAQs |
| **Bina login ke buy** | ✅ Product pe "No login required" note + `checkout.html` guest checkout form (naam, phone, address + COD) |
| **Mobile responsive (her mobile)** | ✅ Bootstrap navbar collapse menu, sticky mobile checkout bar, 44px tap targets, 2-col product grid, stacked hero, safe-area insets |
| **Admin panel changes** | ✅ Neeche list kiye gaye hain |
| **Purani color scheme** | ✅ Same light premium theme rakhi: warm white + charcoal + gold (Playfair Display + Inter) – sirf polish ki |

---

## Hero Section – Existing `rn-slider` Improvements

Existing slider structure **bilkul preserve** hai (full-image + text panel + arrows + dots). Yeh improvements suggest/apply ki hain:

1. **Apparel-focused slides** – "SUMMER LAWN SUITS", "KHADDAR & WOOL", "EID SUITS", "ORDER IN 2 MINUTES" (abhi headphones/laptops pada hai `Views/Products/Index.cshtml:17-20`)
2. **Dual CTA** – primary "Shop Summer" + ghost "View Collection"
3. **Slide counter** (`01 / 04`) + **auto-play progress bar**
4. **Price badge** – "Starting from PKR 1,999"
5. **Stronger gradient overlay** – text kabhi image pe read na ho
6. **Mobile layout** – image upar, text neeche (stack), arrows hidden, swipe supported
7. **Last slide = trust slide** – "No login · COD · 7-day exchange" (local sales motivation)

---

## Admin Panel – Kya Changes Chahiye

### 1. Add/Edit Product (`Views/Admin/Add.cshtml`, `Edit.cshtml`)
| Abhi | Badal kar |
|---|---|
| Category datalist: `Laptops, Phones, Audio...` | **Season select:** `Summer Suit` / `Winter Suit` (future ke liye option list) |
| Brand text field (Asus waghera) | **Fabric dropdown:** Lawn, Cotton, Chiffon, Khaddar, Wool, Cambric, Jacquard |
| Size/Color fields nahi hain | **Sizes (checkboxes):** XS S M L XL XXL · **Colors (comma text / color picker)** |
| Stitched nahi | **Stitched / Unstitched / Semi-Stitched** select |
| Delivery hardcoded free | **Free delivery toggle** + charge amount (jab free na ho) |
| Ek hi `imageFile` | **Up to 5 images** (multiple) + cover + reorder/delete thumbs |
| Placeholder "Gaming Laptop Pro" | "e.g. Printed Lawn 3-Piece Suit" |

### 2. Product Model (`Models/Product.cs`) – Migration chahiye
```csharp
public string? Season { get; set; }      // "Summer" | "Winter"
public string? Fabric { get; set; }      // "Lawn" | "Khaddar" ...
public string? Sizes { get; set; }       // "S,M,L,XL" (comma separated)
public string? Colors { get; set; }      // "Sand,Charcoal"
public int? Pieces { get; set; }         // 2 | 3 (optional)
public string? StitchedType { get; set; } // "Stitched" | "Unstitched" | "Semi-Stitched"
public bool IsFreeDelivery { get; set; } = true;
public decimal? DeliveryCharge { get; set; } // jab IsFreeDelivery = false
public string? ImagePath { get; set; }   // purana primary image (compat)
```

**Product Gallery – ~5 images per suit:**
```csharp
// Models/ProductImage.cs (nayi entity)
public class ProductImage {
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ImagePath { get; set; }
    public int SortOrder { get; set; }   // 0 = primary/cover
    public Product? Product { get; set; }
}
// Product.cs mein: public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
```
- Admin Add/Edit: **multi-file upload** (`<input type="file" multiple>`, max 5) + reorder/delete existing thumbs
- Pehli image = cover (product card); baaki detail gallery mein thumbs/slideshow
- Public Detail: main image + 4 thumbs (jaise `ui-redesign/product.html` ka pd-gallery)
- Migration: `ProductImages` table + optional backfill: agar purana `ImagePath` ho to SortOrder 0 par ek row banayein

### 3. Guest Checkout (sabse bara change)
**Problem:** Abhi `CartController` aur `OrdersController` dono `UserId == null` par Login pe redirect karte hain:
- `Controllers/CartController.cs:20, 51`
- `Controllers/OrdersController.cs:34, 68`

**Solution options:**
- **A (recommended):** Guest cart **cookie/session** mein rakhein (`GuestCartId` GUID), checkout par order create karein
- **B:** Checkout se pehle ek **lightweight guest user** auto-create karein (name/phone se)

**Model changes:**
```csharp
// Order.cs
public int? UserId { get; set; }          // null = guest
public string GuestName { get; set; }
public string GuestPhone { get; set; }
public string? GuestEmail { get; set; }
```

**Checkout form** (`Views/Orders/Checkout.cshtml`) mein email `readonly` mat rakhein – guest ke liye naam + phone input chahiye (jaise `checkout.html` prototype mein hai).

### 4. Delivery Charge (admin-set)
- `SiteSettings` (single row) ya config: free delivery on/off + amount + optional threshold
- `Order.DeliveryCharge` column; `CheckoutService` mein Total = Subtotal + Delivery
- Hardcoded FREE hatao: `Views/Cart/Index.cshtml:60`, `Views/Orders/Checkout.cshtml:164`

### 5. Orders List (`Views/Admin/Orders.cshtml`)
- Customer column: `order.User?.FullName` – guest orders ke liye `GuestName/GuestPhone` fallback chahiye
- Phone column: `GuestPhone` fallback
- Delivery charge column + status dropdown mein `shipped` / `cancelled`

### 6. Categories / Filters
- Home filters (`Views/Products/Index.cshtml`) ab sirf **Season** (All/Summer/Winter) + **Fabric** + **Size** dikhayein
- Brand filter hata sakte hain (brands nahi hain abhi)
- Optional: nayi `Categories` table + admin CRUD (ya seed: Summer Suit / Winter Suit)

### 7. Product Gallery (5 images)
- `ProductImage` table (ProductId, ImagePath, SortOrder)
- Admin: multi-upload max 5 + delete/reorder
- Detail page: cover + thumbs gallery
- Cards/related: SortOrder = 0 wali image

### 8. Hero Slides Admin (`Views/Admin/HeroSlides.cshtml`)
- Functionality theek hai – sirf **content** ladies suits wala karein
- Fallback slides (`Products/Index.cshtml:17-20`) replace karein

### 9. About Page (`Views/Home/About.cshtml`)
- Poori tarah **BaazWix/AI chatbot** wali content hata kar Amal Collection ki nayi copy lagayein (jaise `about.html` mein hai)

### 10. Branding
- `_Layout.cshtml` mein "BaazWix" → "Amal Collection" (title, footer, chatbot name, meta description)
- Logo: `wwwroot/images/baazwix-logo.png` → naya logo

### 11. Site Settings (naya admin page)
- Free delivery on/off, default charge, free-above threshold
- COD on/off (future), contact WhatsApp/phone

### Execution order
1. Product model + ProductImage + migration + seed  
2. Admin Add/Edit forms (season/fabric/size/color/stitched/delivery/5 images)  
3. Delivery charge → CheckoutService + cart/checkout views  
4. Size/Color → Detail → Cart → OrderItem  
5. Public filters + branding  
6. Guest checkout  
7. Categories CRUD (optional) + Settings page

---

## Naye Sections (Homepage)

1. **Announcement bar** – COD available · No login · 7-Day Exchange
2. **Hero slider** (improved existing)
3. **Season cards** – sirf 2: Summer Suits + Winter Suits
4. **Promo banners** – 2-up (Summer Sale + New Khaddar)
5. **Product grid** – hover pe Quick Add, wishlist, discount badge, color dots, season badge
6. **Season filter + Size chips** – All/Summer/Winter · S–XXL
7. **How to Order** – 3 steps (guest flow explain)
8. **Trust strip** – COD · 7-Day Exchange · Local Delivery · **No Login Needed**
9. **Testimonials** + **Newsletter/WhatsApp**
10. **Footer** – WhatsApp contact

---

## Kaise apply karein main site par?

1. **Colors/Fonts**: `wwwroot/css/site.css` ke `:root` variables (lines 5–29) + Google Fonts link `_Layout.cshtml:21` mein Playfair Display add karein
2. **Hero**: `Views/Products/Index.cshtml` ke fallback slides (17–20) clothing wale text se badlein; `rn-*` CSS (`site.css:1200+`) mein counter/progress/overlay improve karein
3. **Branding**: `_Layout.cshtml` + footer + About page
4. **Guest checkout**: `CartController`/`OrdersController` se login redirect hatayein, `Order.UserId` nullable karein, checkout form guest fields dein
5. **Product model**: Season/Fabric/Sizes/Colors fields + migration
6. **Categories**: DB mein sirf Summer Suit / Winter Suit
7. **New pages**: `Views/Home/About.cshtml` rewrite + `Views/Orders/GuestCheckout.cshtml`

Prototype ki styling `css/style.css` mein hai – directly copy-paste karke main `site.css` mein merge ki ja sakti hai.
