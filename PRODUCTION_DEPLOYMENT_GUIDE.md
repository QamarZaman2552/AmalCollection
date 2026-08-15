# 📋 Step-by-Step Production Deployment Guide

> Follow this guide **in order**. Each step depends on the previous one.

---

## 🎯 Prerequisites

- [ ] GitHub account with repo access
- [ ] Azure / AWS / DigitalOcean / VPS account (for hosting)
- [ ] Domain name (optional but recommended)
- [ ] Docker Hub account (for CI/CD)
- [ ] Gmail account (for SMTP emails)

---

## Step 1: Rotate Gmail App Password (5 min)

### Why?
The old password is in git history. Anyone who clones the repo can read your emails.

### How:

1. Go to [Google Account Security](https://myaccount.google.com/security)
2. Enable **2-Step Verification** if not already on
3. Search **"App passwords"** → Click
4. Select **Mail** → **Other (Custom name)** → Type `ShopAI Production`
5. **Copy the 16-character password** (e.g., `abcd efgh ijkl mnop`)
6. **Save it securely** (password manager, notepad) — you'll need it in Step 3

---

## Step 2: Purge Secrets from Git History (10 min)

### Why?
Even after rotating the password, the old one stays in git history forever unless you rewrite history.

### Option A: git-filter-repo (Recommended - Fast & Clean)

```bash
# 1. Install (one-time)
# Windows (via pipx):
pipx install git-filter-repo

# macOS:
brew install git-filter-repo

# Linux:
pip3 install git-filter-repo

# 2. Clone fresh (don't use your current repo!)
cd /tmp
git clone https://github.com/QamarZaman2552/MyProject.git ShopAI-clean
cd ShopAI-clean

# 3. Remove appsettings.json from ALL history
git filter-repo --path appsettings.json --invert-paths

# 4. Verify it's gone
git log --all --full-history -- appsettings.json
# Should show: "fatal: no such path"

# 5. Force push (DESTRUCTIVE - overwrites remote history!)
git push origin --force --all
git push origin --force --tags
```

### Option B: BFG Repo-Cleaner (Alternative)

```bash
# 1. Download bfg.jar from https://rtyley.github.io/bfg-repo-cleaner/
# 2. Clone fresh
git clone --mirror https://github.com/QamarZaman2552/MyProject.git
cd MyProject.git

# 3. Run BFG
java -jar bfg.jar --delete-files appsettings.json

# 4. Clean up
git reflog expire --expire=now --all
git gc --prune=now --aggressive

# 4. Force push
git push --force
```

### ⚠️ After Force Push:
- **All collaborators must re-clone** the repo
- Any open PRs will be broken
- Coordinate with your team if working with others

---

## Step 3: Set Production Environment Variables (15 min)

### Where to Set Them:
| Platform | Location |
|----------|----------|
| **Azure App Service** | Configuration → Application settings |
| **AWS Elastic Beanstalk** | Configuration → Software → Environment properties |
| **DigitalOcean App Platform** | Settings → Environment Variables |
| **VPS (Docker)** | `docker-compose.yml` or `.env` file |
| **GitHub Actions** | Repo Settings → Secrets → Actions (for CI/CD only) |

### Required Variables (Copy-Paste Template):

```bash
# Database (REQUIRED)
ConnectionStrings__dbcs=Server=tcp:YOUR_SQL_SERVER.database.windows.net,1433;Database=ShopAI;User=sqladmin;Password=YOUR_STRONG_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;

# Email (REQUIRED for contact form, password reset)
Email__Enabled=true
Email__Host=smtp.gmail.com
Email__Port=587
Email__EnableSsl=true
Email__UserName=your-email@gmail.com
Email__Password=YOUR_NEW_16_CHAR_APP_PASSWORD
Email__FromEmail=your-email@gmail.com
Email__FromName=ShopAI

# ASP.NET Core (REQUIRED)
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080;https://+:8081
```

### GitHub Actions Secrets (For CI/CD Only):
Go to: **GitHub → Your Repo → Settings → Secrets and variables → Actions → New repository secret**

| Secret Name | Value |
|-------------|-------|
| `DOCKERHUB_USERNAME` | Your Docker Hub username |
| `DOCKERHUB_TOKEN` | Docker Hub Access Token (create at hub.docker.com → Settings → Security → Access Tokens) |

---

## Step 4: Provision Production Database (20 min)

### Option A: Azure SQL Database (Recommended for .NET)

1. **Azure Portal** → Create a resource → **SQL Database**
2. **Server**: Create new → Unique name (e.g., `shopai-prod-sql`)
   - Admin: `sqladmin` / **Strong password** (save it!)
   - Location: Same as your app
   - **Allow Azure services to access**: Yes
3. **Database**: Name `ShopAI` → **Basic** tier (cheapest) or **Standard S0**
4. **Firewall**: Add client IP → Your current IP
5. **Connection String** (copy from Azure → Connection strings → ADO.NET):
   ```
   Server=tcp:shopai-prod-sql.database.windows.net,1433;Database=ShopAI;User=sqladmin;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
   ```

### Option B: AWS RDS SQL Server

1. **RDS Console** → Create database → **SQL Server** → **Free tier** or **db.t3.micro**
2. **Master username**: `admin` / **Strong password**
3. **VPC Security Group**: Allow inbound 1433 from your app's security group
4. **Endpoint**: Copy endpoint (e.g., `shopai-prod.xyz.us-east-1.rds.amazonaws.com,1433`)

### Option C: Self-Hosted (VPS / Docker)

```yaml
# Add to docker-compose.yml
db:
  image: mcr.microsoft.com/mssql/server:2022-latest
  environment:
    - ACCEPT_EULA=Y
    - SA_PASSWORD=YourStrong@Passw0rd
    - MSSQL_PID=Express
  volumes:
    - sql-data:/var/opt/mssql
  ports:
    - "1433:1433"
```

---

## Step 5: Run Migrations on Production DB (5 min)

### Local Machine (with prod connection string):

```bash
# 1. Set the production connection string temporarily
# PowerShell:
$env:ConnectionStrings__dbcs = "Server=tcp:your-server,1433;Database=ShopAI;User=sqladmin;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;"

# Bash:
export ConnectionStrings__dbcs="Server=tcp:your-server,1433;Database=ShopAI;User=sqladmin;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;"

# 2. Navigate to project
cd /path/to/ShopAI

# 3. Apply migrations
dotnet ef database update --project ShoppingApp.csproj

# 4. Verify (optional)
dotnet ef migrations list --project ShoppingApp.csproj
```

### Expected Output:
```
Build started...
Build succeeded.
Applying migration '20260415173325_InitialCreate'.
Applying migration '20260418205141_AddContactMessages'.
...
Applying migration '20260718090520_AddIndexesAndConstraints'.
Done.
```

### Verify Seed Data:
- Admin user: `admin@shop.com` / `Admin@123`
- 10 sample products in DB

---

## Step 6: Configure SSL Certificate (15 min)

### Option A: Azure App Service (Easiest - Free)

1. **App Service** → **Custom domains** → **Add custom domain**
2. Enter your domain (e.g., `shopai.yourdomain.com`)
3. **Add binding** → **SNI SSL** → **Managed Certificate** (free, auto-renew)
4. **HTTPS Only** → **On**

### Option B: Cloudflare (Free, Works Anywhere)

1. **Cloudflare Dashboard** → Add your domain
2. **DNS** → Add `A` record → `@` → Your server IP → **Proxy ON** (orange cloud)
3. **SSL/TLS** → **Full (strict)**
4. **Edge Certificates** → **Always Use HTTPS** → **On**
5. **Automatic HTTPS Rewrites** → **On**

### Option C: Let's Encrypt (VPS / Docker)

```bash
# Install certbot
sudo apt install certbot python3-certbot-nginx

# Get certificate
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com

# Auto-renewal (test)
sudo certbot renew --dry-run
```

### Option D: Docker (with Traefik / Nginx Proxy)

```yaml
# docker-compose.yml addition
proxy:
  image: traefik:v3.0
  command:
    - "--certificatesresolvers.letsencrypt.acme.email=your@email.com"
    - "--certificatesresolvers.letsencrypt.acme.storage=/letsencrypt/acme.json"
    - "--certificatesresolvers.letsencrypt.acme.httpchallenge.entrypoint=web"
  ports:
    - "80:80"
    - "443:443"
  volumes:
    - "./letsencrypt:/letsencrypt"
    - "/var/run/docker.sock:/var/run/docker.sock:ro"
```

---

## Step 7: Deploy (Push to Main)

### Final Checklist Before Push:
- [ ] Gmail password rotated
- [ ] Git history purged (force pushed)
- [ ] Environment variables set on hosting
- [ ] Database migrated
- [ ] SSL configured
- [ ] GitHub secrets added

### Push:
```bash
# 1. Add all changes
git add -A

# 2. Commit
git commit -m "Production ready: security, tests, CI/CD, Docker"

# 3. Push to main (triggers CI/CD)
git push origin master
```

### Watch CI/CD:
1. Go to **GitHub → Actions**
2. Watch "CI/CD Pipeline" workflow
3. Should pass: Build → Test → Security Scan → Docker Build → Deploy

---

## Step 8: Verify Production (10 min)

### After Deployment Completes:

| Check | Command / URL |
|-------|---------------|
| Health live | `curl https://yourdomain.com/health/live` |
| Health ready | `curl https://yourdomain.com/health/ready` |
| Full health | `curl https://yourdomain.com/health` |
| Homepage | Open `https://yourdomain.com` |
| Admin login | `https://yourdomain.com/Auth/Login` → `admin@shop.com` / `Admin@123` |
| Security headers | `curl -I https://yourdomain.com` |

### Expected Security Headers:
```
Strict-Transport-Security: max-age=31536000
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Content-Security-Policy: default-src 'self'...
Referrer-Policy: strict-origin-when-cross-origin
```

### Manual Smoke Tests:
- [ ] Register new user → lands on products
- [ ] Login → session cookie has `__Host-` prefix
- [ ] Add to cart → badge increments
- [ ] Checkout → order confirmation
- [ ] Chatbot → responds to "hello"
- [ ] Contact form → success message
- [ ] 5 failed logins → account locked

---

## 🚨 Troubleshooting

| Issue | Fix |
|-------|-----|
| `dotnet ef database update` fails | Check connection string, firewall, SQL Server running |
| CI/CD fails at Docker push | Verify `DOCKERHUB_USERNAME` and `DOCKERHUB_TOKEN` secrets |
| SSL not working | Check Cloudflare proxy (orange cloud), Azure binding |
| 500 error on app | Check logs: `logs/shopai-*.log` or Azure Log Stream |
| Rate limiting too aggressive | Adjust `PermitLimit` in `Program.cs` |
| Session not persisting | Verify `SameSite=Strict` + HTTPS, cookie domain |

---

## 📞 Quick Reference Commands

```bash
# View logs locally
tail -f logs/shopai-*.log

# Check migration status
dotnet ef migrations list

# Rollback migration
dotnet ef database update PreviousMigrationName

# Force push after history rewrite
git push origin --force --all

# Docker logs
docker-compose logs -f app

# Test rate limit
for i in {1..105}; do curl -s -o /dev/null -w "%{http_code}\n" https://yourdomain.com/health/live; done | sort | uniq -c
```

---

## ✅ You're Live!

Once all checks pass, your ShopAI e-commerce platform is production-ready at **https://yourdomain.com**.

**Admin**: `admin@shop.com` / `Admin@123` (change password after first login!)

---

*Save this guide. You'll need it for future deployments.*