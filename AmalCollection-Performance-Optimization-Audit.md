# AmalCollection Website — Performance Optimization Audit

## Project
**Project:** AmalCollection  
**Purpose:** Performance, smoothness, loading speed, database/API efficiency, and mobile optimization.

---

## Executive Summary

The AmalCollection project is workable, but several areas can be optimized to make the website noticeably smoother and faster.

The highest-impact areas are:

1. Image optimization
2. Homepage database/query optimization
3. Recommendation service optimization
4. Product pagination
5. Lazy loading for product images
6. CSS cleanup
7. Font loading optimization

Static-file caching and several database indexes are already configured well and should be preserved.

---

# 1. Image Optimization — HIGH PRIORITY

## Current Observation

The `wwwroot/images` folder contains approximately:

- **105 images**
- Approximately **20.2 MB** total image size
- Several images are around **500–670 KB**
- Some images are around **1024×1024 or larger**
- Some images are around **1300–1600 px**

Large images increase:

- Initial page load time
- Mobile data usage
- Browser decoding time
- Memory usage
- Largest Contentful Paint (LCP)

## Recommended Changes

Convert suitable images to:

- WebP
- AVIF where practical

Recommended approximate targets:

| Image Type | Target Size |
|---|---:|
| Product card | 50–120 KB |
| Hero image | 150–250 KB |
| Promotional image | 80–150 KB |
| Product detail image | 150–300 KB |

Also resize images according to their actual display size instead of storing unnecessarily large originals.

## Priority

**Very High**

---

# 2. ProductsController Homepage Queries — HIGH PRIORITY

## Current Observation

The homepage loads multiple datasets during a request, including:

- Products
- Recommendations
- Hero slides
- Promotional cards
- Categories
- Seasons
- Fabrics

The following type of pattern was identified:

```csharp
var products = query.ToList();
```

This loads all matching products into memory.

## Problem

As the number of products increases, the amount of data retrieved and processed will increase.

## Recommended Changes

Use:

- Pagination
- Projection with `Select`
- `AsNoTracking()` for read-only queries
- Database-side filtering
- Database-side sorting
- Database-side `Distinct()`

Avoid loading complete entities when only a few fields are required.

## Priority

**High**

---

# 3. GetFilterValues() Optimization — HIGH PRIORITY

## Current Observation

The filter logic contains a pattern similar to:

```csharp
.Select(p => selector(p))
.ToList()
```

followed by filtering/distinct processing in application memory.

## Problem

The database should perform filtering and distinct operations whenever possible.

## Recommended Approach

Instead of:

```csharp
var values = query
    .ToList()
    .Select(...)
    .Distinct();
```

prefer database-side operations such as:

```csharp
var values = await query
    .Select(...)
    .Distinct()
    .ToListAsync();
```

The exact implementation should be adjusted according to the actual property/type being selected.

## Priority

**High**

---

# 4. RecommendationService — HIGH PRIORITY

## Current Observation

The recommendation service contains logic similar to:

```csharp
var all = _db.Products
    .Where(p => p.Stock > 0)
    .ToList();

return all
    .OrderBy(_ => _rng.Next())
    .Take(count)
    .ToList();
```

## Problem

This loads all matching products into memory before selecting the required number.

For example:

- 50 products → manageable
- 5,000 products → unnecessary memory/data processing
- 50,000 products → potentially expensive

Random ordering using:

```csharp
OrderBy(_ => _rng.Next())
```

can also become expensive for large datasets.

## Recommended Changes

The recommendation logic should return only the required number of products.

Possible approaches:

- Database-side selection
- Random IDs
- Random offset/selection
- Cached recommendation pools
- Category-based recommendations
- Recently viewed/product-based recommendations

The best implementation should depend on the intended recommendation behavior.

## Priority

**High**

---

# 5. Product Pagination — HIGH PRIORITY AS DATA GROWS

## Current Observation

The homepage/product listing currently uses a pattern equivalent to:

```csharp
var products = query.ToList();
```

## Problem

There is no effective limit on the number of products retrieved.

If the store grows to hundreds or thousands of products, the page can become slower.

## Recommended Changes

Implement pagination.

Example:

```csharp
var pageSize = 24;

var products = await query
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

Possible page sizes:

- 12
- 16
- 24

For an e-commerce site, **24 products per page** is a reasonable starting point.

## Alternative

Use a **Load More** button or infinite scroll if the UI requires it.

## Priority

**High**

---

# 6. Product Image Lazy Loading — HIGH PRIORITY FOR MOBILE

## Current Observation

Product card images are generally loaded normally.

Example:

```html
<img src="..." />
```

## Recommended Change

For non-critical product images:

```html
<img src="..."
     loading="lazy"
     decoding="async"
     alt="Product name">
```

This allows the browser to delay loading images that are not currently visible.

## Important

Do not blindly lazy-load the main above-the-fold hero image.

The first important hero/LCP image should normally load immediately.

## Priority

**High**

---

# 7. CSS Optimization — MEDIUM/HIGH PRIORITY

## Current Observation

Main CSS files include approximately:

- `site.css` — ~93 KB
- `prototype.css` — ~78 KB
- `pages.css` — ~20 KB

Combined source size is roughly:

**190 KB**

These stylesheets are loaded through the layout.

## Potential Issues

- Duplicate styles
- Unused styles
- Page-specific styles loaded globally
- Repeated rules
- Overly broad selectors

## Recommended Changes

Audit and remove:

- Unused CSS
- Duplicate rules
- Old prototype styles
- Page-specific styles from global bundles

Where practical, split page-specific CSS.

## Priority

**Medium–High**

---

# 8. Bootstrap Source Maps / Development Files

## Observation

Bootstrap-related source map files are present.

## Recommendation

For production deployment, unnecessary development/source-map files can be excluded if they are not needed for production debugging.

This mainly reduces deployment/static-file size rather than directly improving runtime performance.

## Priority

**Low–Medium**

---

# 9. Google Fonts Optimization

## Current Observation

The layout loads fonts from:

```text
fonts.googleapis.com
fonts.gstatic.com
```

External font requests can add network overhead.

## Recommended Improvements

Use:

```html
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
```

Also:

- Load only required font families
- Load only required font weights
- Consider self-hosting fonts if appropriate
- Avoid loading unnecessary variants

## Priority

**Medium**

---

# 10. Static File Caching — ALREADY GOOD

## Current Observation

Static files already use long-term caching similar to:

```text
Cache-Control: public, max-age=31536000, immutable
```

## Recommendation

Keep this configuration.

It is beneficial for:

- CSS
- JavaScript
- Images
- Versioned/static assets

Do not remove it during optimization.

## Status

**Good — Keep as-is**

---

# 11. Database Indexes — MOSTLY GOOD

Several useful indexes already exist for areas such as:

- Cart
- Wishlist
- Reviews
- BrowseHistory
- ContactMessages
- HeroSlides
- PromotionalCards
- PasswordResetTokens

## Recommendation

Do not perform a blind database-index overhaul.

Instead:

1. Identify slow queries
2. Check execution plans
3. Add indexes only where required
4. Monitor production query performance

The current priority is query optimization rather than adding indexes everywhere.

## Status

**Mostly Good**

---

# 12. EF Core Query Optimization

For read-only queries, consider:

```csharp
.AsNoTracking()
```

Example:

```csharp
var products = await _db.Products
    .AsNoTracking()
    .Where(p => p.Stock > 0)
    .Select(p => new ProductListViewModel
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price,
        ImageUrl = p.ImageUrl
    })
    .ToListAsync();
```

Benefits:

- Less EF Core tracking overhead
- Lower memory usage
- Faster read-only operations

Use tracking only where entities actually need to be modified.

---

# 13. Projection Instead of Full Entities

Avoid retrieving an entire entity when the page needs only a few fields.

Instead of:

```csharp
var products = await _db.Products
    .ToListAsync();
```

prefer:

```csharp
var products = await _db.Products
    .AsNoTracking()
    .Select(p => new
    {
        p.Id,
        p.Name,
        p.Price,
        p.ImageUrl
    })
    .ToListAsync();
```

This reduces database-to-application data transfer.

---

# 14. Async Database Operations

For database operations, prefer:

```csharp
ToListAsync()
FirstOrDefaultAsync()
AnyAsync()
CountAsync()
SingleOrDefaultAsync()
```

instead of synchronous equivalents when working inside async controller/service methods.

This helps avoid unnecessary thread blocking.

---

# 15. Mobile Performance

The website should be tested specifically on:

- 4G mobile connection
- Low-end Android device
- Medium Android device
- Desktop
- Different screen sizes

Important areas:

- Hero images
- Product grid
- Navbar
- Filters
- Product detail gallery
- Cart
- Checkout
- Footer

Image optimization and lazy loading are especially important for mobile users.

---

# 16. Recommended Implementation Order

Implement changes in this order:

### Phase 1 — Images

- Convert images to WebP/AVIF
- Resize oversized images
- Compress product images
- Add proper image dimensions

### Phase 2 — Product Loading

- Add pagination
- Add `AsNoTracking()`
- Use projection
- Avoid loading unnecessary columns

### Phase 3 — RecommendationService

- Remove full-table loading
- Remove inefficient random selection
- Return only required products

### Phase 4 — Filter Queries

- Move filtering/distinct operations to SQL
- Use async EF Core methods

### Phase 5 — Frontend

- Add lazy loading
- Add `decoding="async"`
- Optimize hero/LCP image

### Phase 6 — CSS

- Remove duplicate styles
- Remove unused CSS
- Separate page-specific CSS where useful

### Phase 7 — Fonts

- Reduce font weights
- Add preconnect
- Consider self-hosting

### Phase 8 — Testing

Test:

- Homepage
- Product listing
- Product details
- Search
- Filters
- Cart
- Checkout
- Admin pages
- Mobile

---

# 17. Priority Summary

| Priority | Area | Expected Benefit |
|---|---|---|
| 🔴 Critical | Image compression/WebP | Very High |
| 🔴 High | Homepage query optimization | High |
| 🔴 High | RecommendationService | High |
| 🔴 High | Product pagination | High as products grow |
| 🟠 High | Lazy-loaded product images | High on mobile |
| 🟠 Medium/High | CSS cleanup | Medium–High |
| 🟠 Medium | EF Core projection/NoTracking | Medium |
| 🟠 Medium | Font optimization | Medium |
| 🟢 Good | Static caching | Already configured |
| 🟢 Good | Database indexes | Mostly configured |

---

# 18. Important Rule While Optimizing

The optimization work should **not unnecessarily change the existing UI design or business logic**.

Preferred approach:

> Same design + same functionality + same business logic + optimized loading/query behavior.

Changes should be made incrementally and tested after each major optimization.

---

# 19. Final Recommendation

The first optimization pass should focus on:

1. Images
2. Product queries
3. Recommendation queries
4. Pagination
5. Lazy loading
6. CSS cleanup

These areas are most likely to produce visible improvements in website smoothness.

After implementing these changes, performance should be measured again using browser DevTools/Lighthouse and real mobile testing.

---

## Audit Status

**Current status:** Performance audit completed.

**Next recommended step:** File-by-file implementation plan with exact controller, service, view, CSS, and configuration changes.
