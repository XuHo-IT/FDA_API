#!/bin/bash
set -e

API_DLL="FDAAPI.Presentation.FastEndpointBasedApi.dll"

echo "🚀 Starting FDA API container..."

# Chỉ chạy migration nếu có connection string
if [ -n "$ConnectionStrings__MyDB" ]; then
  echo "📦 Running EF Core migrations..."
  dotnet ef database update --no-build
  echo "✅ Database migrations applied."
else
  echo "⚠️ ConnectionStrings__MyDB not set, skipping migrations"
fi

echo "🎯 Launching API..."
exec dotnet /app/$API_DLL
