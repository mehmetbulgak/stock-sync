#!/bin/sh
set -eu
attempt=0
until pg_isready -q -h "$PGHOST" -U stocksync -d stocksync; do
    attempt=$((attempt + 1))
    if [ "$attempt" -ge 60 ]; then
        echo 'PostgreSQL did not become ready.' >&2
        exit 1
    fi
    sleep 1
done
exec dotnet /app/StockSync.Api.dll
