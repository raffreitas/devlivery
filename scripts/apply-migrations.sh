#!/bin/bash
set -e

echo "=========================================="
echo "🚀 Aplicando Migrations - Devlivery WebAPI"
echo "=========================================="

PROJECT_DIR="src/Devlivery.WebApi"

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo ""
echo -e "${YELLOW}📦 Aplicando migrations do ApplicationDbContext...${NC}"
if dotnet ef database update -c ApplicationDbContext --project $PROJECT_DIR --no-build; then
    echo -e "${GREEN}✅ ApplicationDbContext atualizado com sucesso!${NC}"
else
    echo -e "${RED}❌ Erro ao aplicar migrations do ApplicationDbContext${NC}"
    exit 1
fi

echo ""
echo -e "${YELLOW}🔐 Aplicando migrations do ApplicationIdentityDbContext...${NC}"
if dotnet ef database update -c ApplicationIdentityDbContext --project $PROJECT_DIR --no-build; then
    echo -e "${GREEN}✅ ApplicationIdentityDbContext atualizado com sucesso!${NC}"
else
    echo -e "${RED}❌ Erro ao aplicar migrations do ApplicationIdentityDbContext${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}=========================================="
echo "✅ Todas as migrations foram aplicadas!"
echo "==========================================${NC}"
