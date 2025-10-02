#!/bin/bash
set -e

# Ensure devtunnel is installed
if ! command -v devtunnel >/dev/null 2>&1; then
  echo "Error: DevTunnels ('devtunnel') is not installed. Install it from https://aka.ms/devtunnels and try again."
  exit 1
fi

read -r -p "Enter Tunnel name: " tunnelName

devtunnel create "$tunnelName" -a 
devtunnel port create -p 3978  "$tunnelName" 
devtunnel host  "$tunnelName"

