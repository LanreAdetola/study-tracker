#!/usr/bin/env bash
set -euo pipefail

# ── Configuration (override via env vars) ─────────────────────────────────
RESOURCE_GROUP="${RESOURCE_GROUP:-study-tracker-rg}"
LOCATION="${LOCATION:-eastus}"
ENVIRONMENT="${ENVIRONMENT:-prod}"
DEPLOYMENT_NAME="study-tracker-$(date +%Y%m%d%H%M%S)"

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

# ── Main ───────────────────────────────────────────────────────────────────
main() {
  check_prereqs

  log "Checking Azure login..."
  if ! az account show &>/dev/null; then
    warn "Not logged in. Running 'az login'..."
    az login
  fi
  ok "Logged in as: $(az account show --query user.name -o tsv)"

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
