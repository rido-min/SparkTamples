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

read -r -p "Enter BotName: " botName
read -r -p "Enter Azure resource group name: " resourceGroup

appId=$(az bot show --name $botName --resource-group $resourceGroup --query properties.msaAppId -o tsv)
echo "App ID: $appId"

az ad app delete --id $appId
echo "App registration deleted: $appId"

az bot delete --name $botName --resource-group $resourceGroup
echo "Bot Service deleted: $botName in resource group $resourceGroup"