#!/usr/bin/env bash

# Check if pwsh is available
if ! command -v pwsh &> /dev/null; then
  echo "Error: 'pwsh' command not found. Please install PowerShell to proceed."
  exit 1
fi

# Run pwsh with all arguments passed to this script
pwsh -NoProfile ./build.ps1 "$@"

# Pass through the exit code of the pwsh command
exit $?
