#!/bin/bash
set -e

# Tên file DLL của bạn
API_DLL="FDAAPI.Presentation.FastEndpointBasedApi.dll"

echo "🚀 Starting FDA API container..."

# Đợi Postgres khởi động (cách đơn giản)
echo "⏳ Waiting 10s for Postgres to be ready..."
sleep 10

echo "🎯 Launching API..."
exec dotnet "$API_DLL"