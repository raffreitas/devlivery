#!/bin/sh
set -e

# Substituir variável de ambiente BACKEND_URL no nginx.conf
if [ -n "$BACKEND_URL" ]; then
    echo "Configurando BACKEND_URL: $BACKEND_URL"
    sed -i "s|\$BACKEND_URL|$BACKEND_URL|g" /etc/nginx/conf.d/default.conf
else
    echo "AVISO: BACKEND_URL não definida, usando valor padrão"
    # Fallback para desenvolvimento local
    sed -i "s|\$BACKEND_URL|http://localhost:5000|g" /etc/nginx/conf.d/default.conf
fi

# Log da configuração final (debug)
echo "=== Configuração do Nginx ==="
grep -A 5 "proxy_pass" /etc/nginx/conf.d/default.conf || echo "Proxy config não encontrada"

# Testar configuração do Nginx antes de iniciar
nginx -t

# Executar comando original (nginx)
exec "$@"
