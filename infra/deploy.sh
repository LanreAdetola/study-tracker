#!/usr/bin/env bash
set -euo pipefail

# ── Configuration (override via env vars) ─────────────────────────────────
RESOURCE_GROUP="${RESOURCE_GROUP:-study-tracker-rg}"
LOCATION="${LOCATION:-germanywestcentral}"
ENVIRONMENT="${ENVIRONMENT:-prod}"
DEPLOYMENT_NAME="study-tracker-$(date +%Y%m%d%H%M%S)"

# OAuth credentials for App Service Authentication (Easy Auth). Leave unset to
# reuse whatever is already configured on the App Service (see
# load_existing_oauth_config below), or export before running to add/change a
# provider, e.g.:
#   GITHUB_CLIENT_ID=... GITHUB_CLIENT_SECRET=... ./deploy.sh
GITHUB_CLIENT_ID="${GITHUB_CLIENT_ID:-}"
GITHUB_CLIENT_SECRET="${GITHUB_CLIENT_SECRET:-}"
AAD_CLIENT_ID="${AAD_CLIENT_ID:-}"
AAD_CLIENT_SECRET="${AAD_CLIENT_SECRET:-}"

APP_NAME="study-tracker-${ENVIRONMENT}-app"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# ── Helpers ────────────────────────────────────────────────────────────────
log()  { echo -e "\033[1;34m[INFO]\033[0m  $*"; }
ok()   { echo -e "\033[1;32m[ OK ]\033[0m  $*"; }
warn() { echo -e "\033[1;33m[WARN]\033[0m  $*"; }
err()  { echo -e "\033[1;31m[ERR ]\033[0m  $*" >&2; }

# ── Prereq checks ──────────────────────────────────────────────────────────
check_prereqs() {
  local missing=0
  for cmd in az jq; do
    if ! command -v "$cmd" &>/dev/null; then
      err "Required tool not found: $cmd"
      missing=1
    fi
  done
  [[ $missing -eq 0 ]] || { err "Install missing tools and retry."; exit 1; }
}

# ── Preserve OAuth config across partial re-deploys ─────────────────────────
# Bicep deployments replace the full app settings/auth config each run, so a
# deploy that only sets one provider's env vars would otherwise blank out any
# other provider already configured. Fill in unset env vars from what's
# currently live on the App Service instead of leaving them empty.
load_existing_oauth_config() {
  if ! az webapp show --name "$APP_NAME" --resource-group "$RESOURCE_GROUP" &>/dev/null; then
    return 0
  fi

  log "App Service already exists — checking for OAuth config to preserve..."

  local existing_github_id existing_aad_id
  existing_github_id=$(az webapp auth show --name "$APP_NAME" --resource-group "$RESOURCE_GROUP" --query "gitHubClientId" -o tsv 2>/dev/null || echo "")
  existing_aad_id=$(az webapp auth show --name "$APP_NAME" --resource-group "$RESOURCE_GROUP" --query "clientId" -o tsv 2>/dev/null || echo "")

  local existing_settings
  existing_settings=$(az webapp config appsettings list --name "$APP_NAME" --resource-group "$RESOURCE_GROUP" -o json 2>/dev/null || echo "[]")
  local existing_github_secret existing_aad_secret
  existing_github_secret=$(echo "$existing_settings" | jq -r '.[] | select(.name=="GITHUB_CLIENT_SECRET") | .value // ""')
  existing_aad_secret=$(echo "$existing_settings" | jq -r '.[] | select(.name=="AAD_CLIENT_SECRET") | .value // ""')

  if [[ -z "$GITHUB_CLIENT_ID" && -n "$existing_github_id" ]]; then
    warn "GITHUB_CLIENT_ID not set — reusing existing value ($existing_github_id) so GitHub login isn't lost."
    GITHUB_CLIENT_ID="$existing_github_id"
    GITHUB_CLIENT_SECRET="${GITHUB_CLIENT_SECRET:-$existing_github_secret}"
  fi

  if [[ -z "$AAD_CLIENT_ID" && -n "$existing_aad_id" ]]; then
    warn "AAD_CLIENT_ID not set — reusing existing value ($existing_aad_id) so Microsoft login isn't lost."
    AAD_CLIENT_ID="$existing_aad_id"
    AAD_CLIENT_SECRET="${AAD_CLIENT_SECRET:-$existing_aad_secret}"
  fi
}

# ── Main ───────────────────────────────────────────────────────────────────
main() {
  check_prereqs

  log "Checking Azure login..."
  if ! az account show &>/dev/null; then
    warn "Not logged in. Running 'az login'..."
    az login
  fi
  ok "Logged in as: $(az account show --query user.name -o tsv)"

  load_existing_oauth_config

  log "Creating resource group: $RESOURCE_GROUP (location: $LOCATION)"
  az group create \
    --name "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --output none
  ok "Resource group ready."

  log "Deploying Bicep template (this takes ~3-5 minutes)..."
  DEPLOYMENT_OUTPUT=$(az deployment group create \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --template-file "$SCRIPT_DIR/main.bicep" \
    --parameters "$SCRIPT_DIR/main.bicepparam" \
    --parameters environmentName="$ENVIRONMENT" \
    --parameters githubClientId="$GITHUB_CLIENT_ID" githubClientSecret="$GITHUB_CLIENT_SECRET" \
    --parameters aadClientId="$AAD_CLIENT_ID" aadClientSecret="$AAD_CLIENT_SECRET" \
    --query "properties.outputs" \
    --output json)

  APP_URL=$(echo "$DEPLOYMENT_OUTPUT" | jq -r '.appUrl.value')
  APP_NAME=$(echo "$DEPLOYMENT_OUTPUT" | jq -r '.appServiceName.value')
  COSMOS_NAME=$(echo "$DEPLOYMENT_OUTPUT" | jq -r '.cosmosAccountName.value')

  ok "Deployment complete."

  echo ""
  echo "╔══════════════════════════════════════════════════════════════╗"
  echo "║               Study Tracker — Deployment Summary             ║"
  echo "╠══════════════════════════════════════════════════════════════╣"
  printf "║  App URL:      %-47s ║\n" "$APP_URL"
  printf "║  App Service:  %-47s ║\n" "$APP_NAME"
  printf "║  Cosmos DB:    %-47s ║\n" "$COSMOS_NAME"
  printf "║  Resource Grp: %-47s ║\n" "$RESOURCE_GROUP"
  echo "╚══════════════════════════════════════════════════════════════╝"
  echo ""
  echo "─── Next steps ────────────────────────────────────────────────"
  echo ""
  echo "  1. Update the GitHub OAuth App and Microsoft Entra App"
  echo "     Registration redirect URIs to:"
  echo "       $APP_URL/.auth/login/github/callback"
  echo "       $APP_URL/.auth/login/aad/callback"
  echo ""
  echo "  2. GitHub Actions deploys new images automatically via the"
  echo "     docker-build-deploy.yml workflow on push to main. It reuses"
  echo "     the existing AZURE_CLIENT_ID / AZURE_TENANT_ID /"
  echo "     AZURE_SUBSCRIPTION_ID OIDC secrets — no new secrets needed"
  echo "     unless the ghcr.io package is private."
  echo ""
}

main "$@"
