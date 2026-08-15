# 🚀 ShopAI Production Deployment Checklist

> **Run this checklist before every production deployment.**

---

## ✅ Pre-Deployment (One-Time Setup)

### 1. Rotate Exposed Secrets
- [ ] **Gmail App Password** - Generate new at [Google App Passwords](https://myaccount.google.com/apppasswords)
- [ ] **Database Password** - Use strong random password for production SQL Server
- [ ] **JWT/Session Keys** - Generate new if using token auth (not currently used)

### 2. Purge Secrets from Git History
```bash
# Option A: git-filter-repo (recommended)
pipx install git-filter-repo
cd ShopAI
git filter-repo --path appsettings.json --invert-paths
git push origin --force --all
git push origin --force --tags

# Option B: BFG Repo-Cleaner
java -jar bfg.jar --delete-files appsettings.json
git reflog expire --expire=now --all && git gc --prune=now --aggressive
git push origin --force --all
```

### 3. Configure Production Infrastructure
- [ ] **SQL Server** - Provision production instance (Azure SQL / AWS RDS / Self-hosted)
- [ ] **SSL Certificate** - Let's Encrypt / Cloudflare / Azure App Service / IIS
- [ ] **Domain DNS** - Point A/CNAME records to hosting
- [ ] **Firewall** - Allow only 443/80 (HTTP→HTTPS redirect)

---

## ✅ Environment Configuration

### Required Environment Variables
| Variable | Required | Example |
|----------|----------|---------|
| `ConnectionStrings__dbcs` | ✅ | `Server=tcp:prod-sql.database.windows.net,1433;Database=ShopAI;User=admin;Password=xxx;Encrypt=True;TrustServerCertificate=False;` |
| `Email__Enabled` | ✅ | `true` |
| `Email__Host` | ✅ | `smtp.gmail.com` |
| `Email__Port` | ✅ | `587` |
| `Email__EnableSsl` | ✅ | `true` |
| `Email__UserName` | ✅ | `noreply@yourdomain.com` |
| `Email__Password` | ✅ | `your-gmail-app-password` |
| `Email__FromEmail` | ✅ | `noreply@yourdomain.com` |
| `Email__FromName` | ✅ | `ShopAI` |
| `ASPNETCORE_ENVIRONMENT` | ✅ | `Production` |
| `ASPNETCORE_URLS` | Optional | `http://+:8080;https://+:8081` |

### GitHub Repository Secrets
Go to: **Settings → Secrets and variables → Actions**

| Secret | Description |
|--------|-------------|
| `DOCKERHUB_USERNAME` | Docker Hub username |
| `DOCKERHUB_TOKEN` | Docker Hub access token |
| `CONNECTION_STRING` | Prod DB connection (optional if using env vars) |
| `AZURE_WEBAPP_PUBLISH_PROFILE` | If deploying to Azure App Service |
| `SSH_PRIVATE_KEY` | If deploying via SSH |

---

## ✅ Database Migration

```bash
# 1. Set production connection string locally (temporarily)
export ConnectionStrings__dbcs="Server=prod;Database=ShopAI;User=sa;Password=xxx;TrustServerCertificate=True;"

# 2. Apply migrations
dotnet ef database update --project ShopAI/ShoppingApp.csproj

# 3. Verify seed data
# - Admin user: admin@shop.com / Admin@123
# - 10 sample products
```

### Post-Migration Verification
- [ ] Admin login works
- [ ] Products visible on homepage
- [ ] Chatbot responds
- [ ] Cart/Checkout flow works

---

## ✅ CI/CD Pipeline Verification

### GitHub Actions (`.github/workflows/ci-cd.yml`)
- [ ] Workflow runs on `push` to `main`
- [ ] Build passes (restore → build → test)
- [ ] Security scan passes (Semgrep)
- [ ] Docker image builds & pushes to Docker Hub
- [ ] Staging deployment works (if configured)
- [ ] Production deployment requires manual approval

### Local Docker Test
```bash
# Build image
docker build -t shopai:local .

# Run with docker-compose
docker-compose -f docker-compose.yml up -d

# Verify
curl http://localhost:8080/health/live
curl http://localhost:8080/health/ready

# Test app
open http://localhost:8080
```

---

## ✅ Security Verification

### Automated Checks
- [ ] Rate limiting active (check `/health` 100x rapidly → 429)
- [ ] HTTPS redirect works (`curl -I http://domain.com` → 301/302 to https)
- [ ] Security headers present:
  ```bash
  curl -I https://yourdomain.com | grep -iE "x-frame|x-content|referrer|content-security|permissions|cross-origin"
  ```
- [ ] CSP blocks inline scripts (check browser console for violations)
- [ ] Session cookie has `__Host-` prefix, Secure, HttpOnly, SameSite=Strict
- [ ] Anti-forgery tokens on all POST forms

### Manual Tests
- [ ] **Auth**: 5 failed logins → account lockout (5 min)
- [ ] **Auth**: Locked account cannot login even with correct password
- [ ] **Cart**: Quantity > stock → error
- [ ] **Checkout**: Insufficient balance → error with amounts
- [ ] **Admin**: Non-admin redirected from `/Admin/*`
- [ ] **Chatbot**: `<script>alert(1)</script>` → sanitized
- [ ] **Contact**: Invalid email → error
- [ ] **XSS**: No reflected XSS in search/chatbot

---

## ✅ Performance & Reliability

- [ ] Static assets cached (check `Cache-Control: public, max-age=31536000, immutable`)
- [ ] Health endpoints respond:
  - `/health/live` → 200 OK
  - `/health/ready` → 200 OK (DB connected)
  - `/health` → full JSON with status
- [ ] Logging works (check `logs/shopai-*.log`)
- [ ] Serilog structured logs in JSON format
- [ ] Error page shows friendly message (not stack trace) in Production

---

## ✅ Post-Deployment Smoke Tests

| Test | Steps | Expected |
|------|-------|----------|
| Homepage loads | Visit `https://yourdomain.com` | Hero slider, products, recommendations |
| User registration | Register new account | Email verification not required, lands on products |
| User login | Login with credentials | Session cookie set, redirect to products |
| Admin login | `admin@shop.com` / `Admin@123` | Redirect to `/Admin/Products` |
| Add to cart | Click "Add to Cart" on product | Cart badge increments |
| Checkout | Fill delivery form, place order | Order confirmation page |
| Chatbot | Ask "hi" / "cheapest laptop" | Relevant response |
| Contact form | Submit message | Success toast, email sent (DevEmailSender logs) |
| Wishlist | Click heart icon | Badge increments, item in `/Wishlist` |
| Orders history | Visit `/Orders/History` | Past orders listed |

---

## ✅ Monitoring Setup (Recommended)

- [ ] **Uptime monitoring** - UptimeRobot / Pingdom / Azure Monitor (check `/health/live`)
- [ ] **Error tracking** - Sentry / Application Insights
- [ ] **Log aggregation** - Seq / ELK / Azure Log Analytics (ingest Serilog files)
- [ ] **Database monitoring** - Query performance, connection pool
- [ ] **Alerting** - 5xx rate > 1%, DB unavailable, disk space > 80%

---

## 📋 Rollback Plan

If deployment breaks production:

```bash
# 1. Revert Docker image
docker tag shopai:previous shopai:latest
docker-compose up -d

# 2. Or revert GitHub Actions
# Go to Actions → Previous successful run → Re-run

# 3. Database rollback (if migration broke)
dotnet ef database update PreviousMigrationName --project ShopAI/ShoppingApp.csproj
```

---

## 📞 Emergency Contacts

| Role | Contact |
|------|---------|
| DevOps / Infra | |
| Database Admin | |
| Security Team | |
| On-call Engineer | |

---

## ✅ Sign-Off

| Check | Verified By | Date |
|-------|-------------|------|
| Secrets rotated & purged | | |
| Environment variables set | | |
| Migrations applied | | |
| CI/CD green | | |
| Security headers verified | | |
| Smoke tests passed | | |
| Monitoring active | | |
| Rollback tested | | |

**Approved for production:** _______________ **Date:** _______________