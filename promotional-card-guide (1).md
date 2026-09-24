# Swipeable Promotional Card Deck — Complete Guide
> **Zarr-style stacked cards** — swipe karo to next card aaye, mobile responsive, admin se manage ho

---

## Step 1: Database Model

**File:** `Models/PromotionalCard.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace YourApp.Models
{
    public class PromotionalCard
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string BrandName { get; set; }

        [Required]
        public int DiscountPercent { get; set; }

        [Required]
        public decimal CurrentPrice { get; set; }

        [Required]
        public decimal OriginalPrice { get; set; }

        [MaxLength(100)]
        public string AvailableOnText { get; set; }

        [MaxLength(100)]
        public string PlatformName { get; set; }

        public string ImagePath { get; set; }
        public string LogoPath { get; set; }

        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        public string BackgroundColor { get; set; } = "#7B4F52";
    }
}
```

---

## Step 2: DbContext

**File:** `Data/ApplicationDbContext.cs`

```csharp
public DbSet<PromotionalCard> PromotionalCards { get; set; }
```

```bash
dotnet ef migrations add AddPromotionalCards
dotnet ef database update
```

---

## Step 3: Admin Controller

**File:** `Controllers/Admin/PromotionalCardController.cs`

```csharp
[Area("Admin")]
public class PromotionalCardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public PromotionalCardController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<IActionResult> Index()
        => View(await _context.PromotionalCards.OrderBy(c => c.SortOrder).ToListAsync());

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PromotionalCard card, IFormFile ImageFile, IFormFile LogoFile)
    {
        if (ModelState.IsValid)
        {
            card.ImagePath = await SaveFile(ImageFile, "card-images");
            card.LogoPath  = await SaveFile(LogoFile,  "card-logos");
            _context.Add(card);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(card);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var card = await _context.PromotionalCards.FindAsync(id);
        return card == null ? NotFound() : View(card);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PromotionalCard card, IFormFile ImageFile, IFormFile LogoFile)
    {
        if (id != card.Id) return NotFound();
        if (ModelState.IsValid)
        {
            var existing = await _context.PromotionalCards.FindAsync(id);
            if (ImageFile != null) existing.ImagePath = await SaveFile(ImageFile, "card-images");
            if (LogoFile  != null) existing.LogoPath  = await SaveFile(LogoFile,  "card-logos");

            existing.BrandName       = card.BrandName;
            existing.DiscountPercent = card.DiscountPercent;
            existing.CurrentPrice    = card.CurrentPrice;
            existing.OriginalPrice   = card.OriginalPrice;
            existing.AvailableOnText = card.AvailableOnText;
            existing.PlatformName    = card.PlatformName;
            existing.IsActive        = card.IsActive;
            existing.SortOrder       = card.SortOrder;
            existing.BackgroundColor = card.BackgroundColor;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(card);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var card = await _context.PromotionalCards.FindAsync(id);
        if (card != null) { _context.Remove(card); await _context.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    private async Task<string> SaveFile(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0) return null;
        var dir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(dir);
        var name = Guid.NewGuid() + Path.GetExtension(file.FileName);
        using var stream = new FileStream(Path.Combine(dir, name), FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/{folder}/{name}";
    }
}
```

---

## Step 4: Home Controller

**File:** `Controllers/HomeController.cs`

```csharp
public async Task<IActionResult> Index()
{
    var cards = await _context.PromotionalCards
        .Where(c => c.IsActive)
        .OrderBy(c => c.SortOrder)
        .ToListAsync();
    return View(cards);
}
```

---

## Step 5: CSS — Stacked Swipeable Cards (Mobile First)

**File:** `wwwroot/css/promo-card.css`

```css
/* ==============================
   PROMO DECK — Mobile First
   ============================== */

.promo-section {
    padding: 40px 16px;
    text-align: center;
}

.promo-section__title {
    font-size: clamp(18px, 4vw, 26px);
    font-weight: 700;
    letter-spacing: 1px;
    margin-bottom: 6px;
    color: #1a1a1a;
}

.promo-section__hint {
    font-size: 13px;
    color: #888;
    margin-bottom: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 6px;
}

/* Deck container */
.promo-deck {
    position: relative;
    width: min(340px, 88vw);
    height: min(420px, 110vw);
    margin: 0 auto 24px;
    min-height: 280px;
}

/* Ghost cards (stacked effect) */
.promo-deck__ghost {
    position: absolute;
    width: 100%;
    height: 100%;
    border-radius: 20px;
    background: rgba(0, 0, 0, 0.18);
}
.promo-deck__ghost--2 {
    top: -10px; left: 8px;
    transform: rotate(-4deg);
    z-index: 1;
}
.promo-deck__ghost--1 {
    top: -5px; left: 4px;
    transform: rotate(-2deg);
    z-index: 2;
}

/* Swipeable card */
.promo-card {
    position: absolute;
    width: 100%;
    height: 100%;
    border-radius: 20px;
    overflow: hidden;
    display: flex;
    box-shadow: 0 10px 40px rgba(0,0,0,0.35);
    z-index: 10;
    cursor: grab;
    user-select: none;
    touch-action: none;
    will-change: transform;
}
.promo-card:active { cursor: grabbing; }

.promo-card:not(:last-child) {
    z-index: 3;
    transform: scale(0.95) translateY(10px);
    pointer-events: none;
    transition: transform 0.35s ease, opacity 0.35s ease;
}

/* Left: image */
.promo-card__img {
    width: 50%;
    height: 100%;
    object-fit: cover;
    object-position: top center;
    flex-shrink: 0;
    pointer-events: none;
}

/* Right: info */
.promo-card__info {
    width: 50%;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    padding: clamp(10px, 4%, 24px);
    text-align: center;
    color: #fff;
    gap: 4px;
}

.promo-card__brand {
    font-size: clamp(8px, 2.2vw, 12px);
    letter-spacing: 2px;
    font-weight: 500;
    opacity: 0.88;
}

.promo-card__discount {
    font-size: clamp(30px, 9vw, 52px);
    font-weight: 900;
    color: #D4AF37;
    line-height: 1;
}
.promo-card__discount span {
    font-size: 0.44em;
    font-weight: 700;
    display: block;
    letter-spacing: 1px;
}

.promo-card__now {
    font-size: clamp(11px, 2.8vw, 15px);
    font-weight: 600;
}

.promo-card__was {
    font-size: clamp(9px, 2.2vw, 12px);
    background: #000;
    color: #fff;
    padding: 2px 8px;
    border-radius: 4px;
    text-decoration: line-through;
    text-decoration-color: #e53935;
}

.promo-card__avail {
    font-size: clamp(7px, 1.8vw, 10px);
    letter-spacing: 1.5px;
    opacity: 0.8;
    margin-top: 8px;
}

.promo-card__logo {
    max-width: clamp(45px, 13vw, 70px);
    max-height: 32px;
    object-fit: contain;
    filter: brightness(0) invert(1);
    opacity: 0.9;
}

.promo-card__platform {
    font-size: clamp(14px, 4.5vw, 20px);
    font-weight: 800;
    letter-spacing: 3px;
}

/* Dots */
.promo-dots {
    display: flex;
    justify-content: center;
    gap: 8px;
    margin-top: 12px;
}
.promo-dot {
    width: 8px; height: 8px;
    border-radius: 50%;
    background: #ccc;
    transition: background 0.3s, transform 0.3s;
}
.promo-dot.active {
    background: #D4AF37;
    transform: scale(1.3);
}

/* Fly animations */
.promo-card--fly-left {
    transition: transform 0.4s ease, opacity 0.4s ease !important;
    transform: translateX(-130%) rotate(-20deg) !important;
    opacity: 0;
}
.promo-card--fly-right {
    transition: transform 0.4s ease, opacity 0.4s ease !important;
    transform: translateX(130%) rotate(20deg) !important;
    opacity: 0;
}

/* ── Responsive ── */
@media (max-width: 360px) {
    .promo-deck { width: 92vw; height: 115vw; min-height: 250px; }
}
@media (min-width: 600px) {
    .promo-deck { width: 380px; height: 480px; }
}
@media (min-width: 1024px) {
    .promo-section { padding: 60px 24px; }
    .promo-deck { width: 420px; height: 520px; }
}
```

---

## Step 6: Razor View — Public Page

**File:** `Views/Home/Index.cshtml`

```html
@model IEnumerable<YourApp.Models.PromotionalCard>

@{ var cards = Model.ToList(); }

<link rel="stylesheet" href="~/css/promo-card.css" />

<section class="promo-section">
    <h2 class="promo-section__title">Swipe the Deck</h2>
    <p class="promo-section__hint">👆 Drag &amp; fling any direction — infinite loop</p>

    <div class="promo-deck" id="promoDeck">
        <div class="promo-deck__ghost promo-deck__ghost--2"></div>
        <div class="promo-deck__ghost promo-deck__ghost--1"></div>

        @for (int i = cards.Count - 1; i >= 0; i--)
        {
            var card = cards[i];
            <div class="promo-card" data-index="@i"
                 style="background-color: @card.BackgroundColor;">

                <img class="promo-card__img"
                     src="@card.ImagePath"
                     alt="@card.BrandName"
                     draggable="false" />

                <div class="promo-card__info">
                    <div class="promo-card__brand">@card.BrandName</div>
                    <div class="promo-card__discount">
                        @card.DiscountPercent%
                        <span>OFF</span>
                    </div>
                    <div class="promo-card__now">Now: Rs.@card.CurrentPrice.ToString("N0")</div>
                    <div class="promo-card__was">Was: Rs.@card.OriginalPrice.ToString("N0")</div>
                    <div class="promo-card__avail">@card.AvailableOnText</div>
                    @if (!string.IsNullOrEmpty(card.LogoPath))
                    {
                        <img class="promo-card__logo" src="@card.LogoPath"
                             alt="@card.PlatformName" draggable="false" />
                    }
                    <div class="promo-card__platform">@card.PlatformName</div>
                </div>
            </div>
        }
    </div>

    <div class="promo-dots" id="promoDots">
        @for (int i = 0; i < cards.Count; i++)
        {
            <div class="promo-dot @(i == 0 ? "active" : "")"></div>
        }
    </div>
</section>

<script src="~/js/promo-swipe.js"></script>
```

---

## Step 7: JavaScript — Swipe Engine

**File:** `wwwroot/js/promo-swipe.js`

```javascript
(function () {
    const deck = document.getElementById('promoDeck');
    const dots = document.querySelectorAll('.promo-dot');
    if (!deck) return;

    let cards        = Array.from(deck.querySelectorAll('.promo-card'));
    const total      = cards.length;
    let currentIndex = 0;
    let isDragging   = false;
    let startX = 0, startY = 0, currentX = 0;
    const THRESHOLD  = 80;

    function getTopCard() {
        return deck.querySelector('.promo-card:last-child');
    }

    function updateDots(idx) {
        dots.forEach((d, i) => d.classList.toggle('active', i === idx));
    }

    function refreshStack() {
        cards = Array.from(deck.querySelectorAll('.promo-card'));
        cards.forEach((c, i) => {
            c.style.transition = 'transform 0.35s ease, opacity 0.35s ease';
            if (i === cards.length - 1) {
                c.style.transform     = 'none';
                c.style.opacity       = '1';
                c.style.zIndex        = '10';
                c.style.pointerEvents = 'auto';
            } else {
                const depth = cards.length - 1 - i;
                c.style.transform     = `scale(${1 - depth * 0.04}) translateY(${depth * 8}px)`;
                c.style.opacity       = depth > 2 ? '0' : '1';
                c.style.zIndex        = i;
                c.style.pointerEvents = 'none';
            }
        });
    }

    function flyCard(card, dir) {
        const cls = dir > 0 ? 'promo-card--fly-right' : 'promo-card--fly-left';
        card.classList.add(cls);
        card.addEventListener('transitionend', function handler() {
            card.removeEventListener('transitionend', handler);
            card.classList.remove(cls);
            card.style.cssText = '';
            deck.insertBefore(card, deck.querySelector('.promo-card'));
            currentIndex = (currentIndex + 1) % total;
            updateDots(currentIndex);
            refreshStack();
        }, { once: true });
    }

    function onStart(e) {
        isDragging = true;
        startX  = e.touches ? e.touches[0].clientX : e.clientX;
        startY  = e.touches ? e.touches[0].clientY : e.clientY;
        currentX = 0;
        const top = getTopCard();
        if (top) top.style.transition = 'none';
    }

    function onMove(e) {
        if (!isDragging) return;
        const x  = e.touches ? e.touches[0].clientX : e.clientX;
        const y  = e.touches ? e.touches[0].clientY : e.clientY;
        currentX = x - startX;
        const dy = y - startY;

        if (Math.abs(dy) > Math.abs(currentX) * 1.5) {
            isDragging = false;
            const top = getTopCard();
            if (top) { top.style.transition = ''; top.style.transform = ''; }
            return;
        }

        if (e.cancelable) e.preventDefault();

        const top = getTopCard();
        if (top) top.style.transform = `translateX(${currentX}px) rotate(${currentX * 0.08}deg)`;
    }

    function onEnd() {
        if (!isDragging) return;
        isDragging = false;
        const top = getTopCard();
        if (!top) return;

        if (Math.abs(currentX) >= THRESHOLD) {
            flyCard(top, currentX);
        } else {
            top.style.transition = 'transform 0.35s ease';
            top.style.transform  = 'none';
        }
        currentX = 0;
    }

    // Mouse
    deck.addEventListener('mousedown',     onStart);
    document.addEventListener('mousemove', onMove);
    document.addEventListener('mouseup',   onEnd);

    // Touch
    deck.addEventListener('touchstart', onStart, { passive: true });
    deck.addEventListener('touchmove',  onMove,  { passive: false });
    deck.addEventListener('touchend',   onEnd);

    refreshStack();
    updateDots(0);
})();
```

---

## Step 8: Admin Views

### Index (List) — `Areas/Admin/Views/PromotionalCard/Index.cshtml`

```html
@model IEnumerable<YourApp.Models.PromotionalCard>
<div class="container mt-4">
    <div class="d-flex justify-content-between align-items-center mb-3">
        <h2>Promotional Cards</h2>
        <a asp-action="Create" class="btn btn-success">+ Naya Card</a>
    </div>
    <table class="table table-bordered table-hover">
        <thead class="table-dark">
            <tr><th>Order</th><th>Brand</th><th>Discount</th><th>Now</th><th>Was</th><th>Active</th><th>Actions</th></tr>
        </thead>
        <tbody>
            @foreach (var c in Model.OrderBy(x => x.SortOrder))
            {
                <tr>
                    <td>@c.SortOrder</td>
                    <td>@c.BrandName</td>
                    <td><strong>@c.DiscountPercent%</strong></td>
                    <td>Rs. @c.CurrentPrice.ToString("N0")</td>
                    <td><s>Rs. @c.OriginalPrice.ToString("N0")</s></td>
                    <td>@(c.IsActive ? "✅" : "❌")</td>
                    <td>
                        <a asp-action="Edit" asp-route-id="@c.Id" class="btn btn-sm btn-warning">Edit</a>
                        <a asp-action="Delete" asp-route-id="@c.Id" class="btn btn-sm btn-danger"
                           onclick="return confirm('Delete?')">Delete</a>
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

### Create Form — `Areas/Admin/Views/PromotionalCard/Create.cshtml`

```html
@model YourApp.Models.PromotionalCard
<div class="container mt-4" style="max-width:600px">
    <h2>Naya Card Banao</h2>
    <form asp-action="Create" method="post" enctype="multipart/form-data">
        <div class="mb-3">
            <label class="form-label">Brand Name</label>
            <input asp-for="BrandName" class="form-control" placeholder="SANA SAFINAZ" />
        </div>
        <div class="row">
            <div class="col mb-3">
                <label class="form-label">Discount %</label>
                <input asp-for="DiscountPercent" type="number" class="form-control" />
            </div>
            <div class="col mb-3">
                <label class="form-label">Sort Order</label>
                <input asp-for="SortOrder" type="number" class="form-control" />
            </div>
        </div>
        <div class="row">
            <div class="col mb-3">
                <label class="form-label">Current Price (Rs.)</label>
                <input asp-for="CurrentPrice" type="number" step="0.01" class="form-control" />
            </div>
            <div class="col mb-3">
                <label class="form-label">Original Price (Rs.)</label>
                <input asp-for="OriginalPrice" type="number" step="0.01" class="form-control" />
            </div>
        </div>
        <div class="mb-3">
            <label class="form-label">Available On Text</label>
            <input asp-for="AvailableOnText" class="form-control" placeholder="AVAILABLE ON" />
        </div>
        <div class="mb-3">
            <label class="form-label">Platform Name</label>
            <input asp-for="PlatformName" class="form-control" placeholder="ZARR" />
        </div>
        <div class="mb-3">
            <label class="form-label">Background Color</label>
            <input asp-for="BackgroundColor" type="color" class="form-control form-control-color" />
        </div>
        <div class="mb-3">
            <label class="form-label">Product Image</label>
            <input type="file" name="ImageFile" accept="image/*" class="form-control" />
        </div>
        <div class="mb-3">
            <label class="form-label">Platform Logo</label>
            <input type="file" name="LogoFile" accept="image/*" class="form-control" />
        </div>
        <div class="form-check mb-3">
            <input asp-for="IsActive" type="checkbox" class="form-check-input" />
            <label class="form-check-label">Card Active Rakho</label>
        </div>
        <button type="submit" class="btn btn-primary">Save</button>
        <a asp-action="Index" class="btn btn-secondary">Cancel</a>
    </form>
</div>
```

---

## Step 9: Folder Structure

```
wwwroot/
├── css/
│   └── promo-card.css
├── js/
│   └── promo-swipe.js
└── uploads/
    ├── card-images/
    └── card-logos/
```

---

## Swipe Behavior Summary

| Action | Result |
|---|---|
| Left swipe (80px+) | Card fly left → next card aata hai |
| Right swipe (80px+) | Card fly right → next card aata hai |
| Chota drag (< 80px) | Card snap back |
| Vertical scroll | Normal scroll, card drag cancel |
| Last card ke baad | First card wapas aata hai (infinite loop) |
| Mobile touch | Fully supported |
| Desktop mouse | Fully supported |
| Dot indicators | Automatically update hote hain |
