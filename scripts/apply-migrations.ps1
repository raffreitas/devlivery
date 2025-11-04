# PowerShell script para aplicar migrations localmente
param(
    [string]$ProjectDir = "src/Devlivery.WebApi"
)

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "🚀 Aplicando Migrations - Devlivery WebAPI" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Aplicar ApplicationDbContext
Write-Host "📦 Aplicando migrations do ApplicationDbContext..." -ForegroundColor Yellow
try {
    dotnet ef database update -c ApplicationDbContext --project $ProjectDir --no-build
    Write-Host "✅ ApplicationDbContext atualizado com sucesso!" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro ao aplicar migrations do ApplicationDbContext" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Aplicar ApplicationIdentityDbContext
Write-Host "🔐 Aplicando migrations do ApplicationIdentityDbContext..." -ForegroundColor Yellow
try {
    dotnet ef database update -c ApplicationIdentityDbContext --project $ProjectDir --no-build
    Write-Host "✅ ApplicationIdentityDbContext atualizado com sucesso!" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro ao aplicar migrations do ApplicationIdentityDbContext" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "✅ Todas as migrations foram aplicadas!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
