#!/bin/bash
set -e

# Ensure Azure CLI is installed
if ! command -v az >/dev/null 2>&1; then
  echo "Error: Azure CLI ('az') is not installed. Install it from https://aka.ms/azcli and try again."
  exit 1
fi

# Ensure Azure CLI is logged in and has a subscription set
subscriptionId=$(az account show --query id -o tsv 2>/dev/null || true)
if [ -z "$subscriptionId" ]; then
  echo "Error: Azure CLI is not logged in or no subscription is set."
  echo "Run 'az login' to sign in, then 'az account set --subscription <id|name>' to set a subscription."
  exit 1
fi

read -r -p "Enter App ID: " appId

botName=$(az ad app show --id $appId --query displayName -o tsv)

appCred=$(az ad app credential reset --id $appId)
tenantId=$(echo $appCred | jq -r '.tenant')
clientSecret=$(echo $appCred | jq -r '.password')
echo "App registration credentials refreshed: $appId"


echo "TENANT_ID=$tenantId" > "$botName.env"
echo "CLIENT_ID=$appId" >> "$botName.env"
echo "CLIENT_SECRET=$clientSecret" >> "$botName.env"

echo "Environment variables saved to $botName.env"