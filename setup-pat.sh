#!/bin/bash

# Azure DevOps PAT Authentication Setup Script
# Usage: ./setup-pat.sh YOUR_PAT_TOKEN

if [ -z "$1" ]; then
    echo "Usage: $0 <YOUR_PAT_TOKEN>"
    echo "Example: $0 abcd1234efgh5678ijkl9012mnop3456qrst7890"
    exit 1
fi

PAT_TOKEN="$1"

# Enable credential provider session token cache
export NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED=true

# Set up the external feed endpoints with PAT
export VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://pkgs.dev.azure.com/DomoreexpGithub/Github_Pipelines/_packaging/TeamsSDKPreviews/nuget/v3/index.json\", \"username\":\"PAT\", \"password\":\"$PAT_TOKEN\"}]}"

echo "✅ PAT authentication configured successfully!"
echo "Environment variables set:"
echo "  - NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED=true"
echo "  - VSS_NUGET_EXTERNAL_FEED_ENDPOINTS configured"
echo ""
echo "Now you can run: dotnet restore"