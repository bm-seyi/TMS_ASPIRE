#!/bin/sh

# Start Vault in development mode
vault server -dev -dev-root-token-id=$VAULT_DEV_ROOT_TOKEN_ID -dev-listen-address=0.0.0.0:8200 &

# Wait for Vault to be fully initialized (increased wait time)
echo "Waiting for Vault to initialize..."
sleep 10

# Export VAULT_ADDR and VAULT_TOKEN for CLI commands
export VAULT_ADDR='http://0.0.0.0:8200'
export VAULT_TOKEN=$VAULT_DEV_ROOT_TOKEN_ID

# Enable PKI secrets engine with max lease TTL
echo "Enabling PKI secrets engine..."
vault secrets enable -max-lease-ttl=87600h pki

# Configure the Root Certificate for PKI
echo "Configuring root certificate..."
vault write pki/root/generate/internal \
    common_name="localhost" \
    ttl=87600h

# Configure the URLs for the PKI engine
echo "Configuring URLs for PKI engine..."
vault write pki/config/urls \
    issuing_certificates="http://0.0.0.0:8200/v1/pki/ca" \
    crl_distribution_points="http://0.0.0.0:8200/v1/pki/crl"

# Create a role for issuing certificates
echo "Creating PKI role..."
vault write pki/roles/my-role \
    allowed_domains="localhost" \
    allow_subdomains=true \
    max_ttl="72h"

# Configure Keycloak PKI role with proper settings
echo "Configuring Keycloak PKI role..."
vault write pki/roles/keycloak \
    allowed_domains="localhost,keycloak" \
    allow_subdomains=true \
    allow_localhost=true \
    allow_bare_domains=true \
    allowed_uri_sans="spiffe://*" \
    max_ttl="8760h"

vault write pki/issue/keycloak common_name="keycloak" alt_names="keycloak.localhost,localhost" ip_sans="127.0.0.1"


# Enable KV secrets engine (optional but recommended)
vault secrets enable -version=2 kv

vault secrets list

# Keep the container running
tail -f /dev/null