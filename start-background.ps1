# Start ShopAI on port 5226 (background/dev use)
# VS uses 5225 — these never conflict.
$ErrorActionPreference = "Stop"
$proj = "D:\Final_Year_Project-BSIT-20260531T111602Z-3-001\Final_Year_Project-BSIT\ShopAI"

# Stop any existing app instances first
Get-Process -Name "ShoppingApp" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep 2

Start-Process -FilePath "dotnet" -ArgumentList "run --urls http://localhost:5226" -WorkingDirectory $proj -WindowStyle Hidden

Write-Host "Starting ShopAI on http://localhost:5226 (VS stays on 5225)"
for ($i = 0; $i -lt 20; $i++) {
    Start-Sleep 1
    try {
        $r = Invoke-WebRequest -Uri "http://localhost:5226/health" -UseBasicParsing -TimeoutSec 3
        Write-Host "HEALTHY: $($r.Content)"
        return
    } catch { }
}
Write-Host "App not responding after 20s - check logs."