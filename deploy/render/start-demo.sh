#!/bin/sh
set -eu
umask 077

mkdir -p "$PGHOST"
if [ ! -s "$PGDATA/PG_VERSION" ]; then
    initdb -D "$PGDATA" -U app --auth-local=peer --auth-host=reject --encoding=UTF8 --no-locale
    cat > "$PGDATA/pg_hba.conf" <<'EOF'
local stocksync stocksync peer map=stocksync
local all app peer
EOF
    printf 'stocksync app stocksync\n' > "$PGDATA/pg_ident.conf"
    cat >> "$PGDATA/postgresql.conf" <<EOF
listen_addresses = ''
unix_socket_directories = '$PGHOST'
unix_socket_permissions = 0700
shared_buffers = '24MB'
max_connections = 25
work_mem = '2MB'
maintenance_work_mem = '16MB'
EOF
    pg_ctl -D "$PGDATA" -l /tmp/stocksync-postgres.log -w start
    psql -U app -d postgres -v ON_ERROR_STOP=1 -c 'CREATE ROLE stocksync LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE;'
    createdb -U app -O stocksync stocksync
    pg_ctl -D "$PGDATA" -m fast -w stop
fi

sed \
    -e 's/listen 8080;/listen ${PORT};/' \
    -e 's|root /usr/share/nginx/html;|root /app/wwwroot;|' \
    -e 's|http://api:8080|http://127.0.0.1:8080|' \
    -e 's|X-Forwarded-Proto $scheme|X-Forwarded-Proto https|' \
    /app/deploy/server.conf.template \
    | envsubst '${PORT}' > /tmp/stocksync-server.conf

echo 'Starting shared StockSync portfolio demo. Data resets when the instance restarts.'
exec supervisord -c /app/deploy/supervisord.conf
