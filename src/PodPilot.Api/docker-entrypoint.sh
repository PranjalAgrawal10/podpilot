#!/bin/sh
set -e

echo "Applying database migrations and seeders..."
dotnet PodPilot.Api.dll --migrate-only

echo "Starting PodPilot API..."
exec dotnet PodPilot.Api.dll
