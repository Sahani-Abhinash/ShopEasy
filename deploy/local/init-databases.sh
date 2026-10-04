#!/bin/bash
# Runs once, on the first start of an empty pgdata volume (docker-entrypoint-initdb.d).
# One database + one user per service (Chapter 1, §6). Passwords come from .env via compose.yaml.
set -euo pipefail

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres \
     -v catalog_pw="$CATALOG_DB_PASSWORD" -v orders_pw="$ORDERS_DB_PASSWORD" <<'EOSQL'
CREATE USER catalog_user WITH PASSWORD :'catalog_pw';
CREATE DATABASE catalog_db OWNER catalog_user;
REVOKE CONNECT ON DATABASE catalog_db FROM PUBLIC;

CREATE USER orders_user WITH PASSWORD :'orders_pw';
CREATE DATABASE orders_db OWNER orders_user;
REVOKE CONNECT ON DATABASE orders_db FROM PUBLIC;
EOSQL
