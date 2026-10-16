-- Least-privilege DBs/users for local Compose.
-- Passwords must match .env (change-me placeholders are fine locally).

CREATE USER inference WITH PASSWORD 'change-me';
CREATE DATABASE inference_db OWNER inference;

CREATE USER identity WITH PASSWORD 'change-me';
CREATE DATABASE identity_db OWNER identity;

CREATE USER tenant WITH PASSWORD 'change-me';
CREATE DATABASE tenant_db OWNER tenant;

CREATE USER policy WITH PASSWORD 'change-me';
CREATE DATABASE policy_db OWNER policy;

CREATE USER catalog WITH PASSWORD 'change-me';
CREATE DATABASE catalog_db OWNER catalog;

CREATE USER budget WITH PASSWORD 'change-me';
CREATE DATABASE budget_db OWNER budget;

CREATE USER provider WITH PASSWORD 'change-me';
CREATE DATABASE provider_db OWNER provider;

CREATE USER notification WITH PASSWORD 'change-me';
CREATE DATABASE notification_db OWNER notification;

CREATE USER analytics WITH PASSWORD 'change-me';
CREATE DATABASE analytics_db OWNER analytics;

CREATE USER audit WITH PASSWORD 'change-me';
CREATE DATABASE audit_db OWNER audit;

-- Revoke cross-db public access as a baseline (each app connects only to its DB).
REVOKE ALL ON DATABASE inference_db FROM PUBLIC;
REVOKE ALL ON DATABASE identity_db FROM PUBLIC;
REVOKE ALL ON DATABASE tenant_db FROM PUBLIC;
REVOKE ALL ON DATABASE policy_db FROM PUBLIC;
REVOKE ALL ON DATABASE catalog_db FROM PUBLIC;
REVOKE ALL ON DATABASE budget_db FROM PUBLIC;
REVOKE ALL ON DATABASE provider_db FROM PUBLIC;
REVOKE ALL ON DATABASE notification_db FROM PUBLIC;
REVOKE ALL ON DATABASE analytics_db FROM PUBLIC;
REVOKE ALL ON DATABASE audit_db FROM PUBLIC;

GRANT CONNECT ON DATABASE inference_db TO inference;
GRANT CONNECT ON DATABASE identity_db TO identity;
GRANT CONNECT ON DATABASE tenant_db TO tenant;
GRANT CONNECT ON DATABASE policy_db TO policy;
GRANT CONNECT ON DATABASE catalog_db TO catalog;
GRANT CONNECT ON DATABASE budget_db TO budget;
GRANT CONNECT ON DATABASE provider_db TO provider;
GRANT CONNECT ON DATABASE notification_db TO notification;
GRANT CONNECT ON DATABASE analytics_db TO analytics;
GRANT CONNECT ON DATABASE audit_db TO audit;