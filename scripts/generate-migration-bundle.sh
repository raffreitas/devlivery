#!/bin/bash
set -e

echo "================================================"
echo "📦 Gerando Migration Bundles - Devlivery WebAPI"
echo "================================================"

PROJECT_DIR="src/Devlivery.WebApi"
OUTPUT_DIR="migration-bundles"

# Criar diretório de output se não existir
mkdir -p $OUTPUT_DIR

echo ""
echo "🔨 Gerando bundle para ApplicationDbContext..."
dotnet ef migrations bundle \
    --project $PROJECT_DIR \
    --context ApplicationDbContext \
    --output $OUTPUT_DIR/efbundle-db \
    --self-contained \
    --configuration Release \
    --force

echo ""
echo "🔨 Gerando bundle para ApplicationIdentityDbContext..."
dotnet ef migrations bundle \
    --project $PROJECT_DIR \
    --context ApplicationIdentityDbContext \
    --output $OUTPUT_DIR/efbundle-identity \
    --self-contained \
    --configuration Release \
    --force

echo ""
echo "✅ Migration bundles gerados com sucesso em: $OUTPUT_DIR/"
echo "   - efbundle-db (ApplicationDbContext)"
echo "   - efbundle-identity (ApplicationIdentityDbContext)"
